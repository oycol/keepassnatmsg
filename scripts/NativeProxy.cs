using System;
using System.IO;
using System.IO.Pipes;

namespace KeePassNatMsgProxy
{
    class Program
    {
        static void Log(string msg)
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeePassNatMsg");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "proxy.log"), string.Format("[{0:HH:mm:ss.fff}] {1}\r\n", DateTime.Now, msg));
            }
            catch { }
        }

        static bool ReadExact(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read <= 0) return false;
                offset += read;
                count -= read;
            }
            return true;
        }

        static int Main(string[] args)
        {
            var pipeName = "keepassxc\\" + Environment.UserName + "\\kpxc_server";
            Log("Proxy started. Target pipe: " + pipeName + ", Args: " + string.Join(" ", args));

            try
            {
                using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    Log("Connecting to named pipe: " + pipeName);
                    pipe.Connect(5000);
                    Log("Connected to pipe successfully!");

                    var stdin = Console.OpenStandardInput();
                    var stdout = Console.OpenStandardOutput();

                    var header = new byte[4];
                    while (true)
                    {
                        // 1. Read 4-byte message length
                        int read = 0;
                        while (read < 4)
                        {
                            int r = stdin.Read(header, read, 4 - read);
                            if (r <= 0)
                            {
                                Log("Stdin EOF detected at header. Exiting.");
                                return 0;
                            }
                            read += r;
                        }
                        int length = BitConverter.ToInt32(header, 0);
                        Log(string.Format("Read header length: {0} bytes", length));

                        if (length <= 0 || length > 10 * 1024 * 1024)
                        {
                            Log(string.Format("Invalid length: {0}. Exiting.", length));
                            return 0;
                        }

                        // 2. Read message body
                        var body = new byte[length];
                        read = 0;
                        while (read < length)
                        {
                            int r = stdin.Read(body, read, length - read);
                            if (r <= 0)
                            {
                                Log("Stdin EOF detected at body. Exiting.");
                                return 0;
                            }
                            read += r;
                        }
                        Log(string.Format("Read full message body: {0} bytes", length));

                        // 3. Forward one complete framed request to the byte-mode pipe.
                        pipe.Write(header, 0, header.Length);
                        pipe.Write(body, 0, body.Length);
                        pipe.Flush();
                        Log("Forwarded message to pipe.");

                        // 4. Read exactly one complete framed response. A pipe Read
                        // can be short even if the writer used a single Write call.
                        if (!ReadExact(pipe, header, 0, header.Length))
                        {
                            Log("Pipe EOF at response header. Exiting.");
                            return 0;
                        }
                        int respLen = header[0] | (header[1] << 8) |
                            (header[2] << 16) | (header[3] << 24);
                        if (respLen <= 0 || respLen > 10 * 1024 * 1024)
                        {
                            Log("Invalid response length: " + respLen);
                            return 0;
                        }
                        var respBuf = new byte[respLen];
                        if (!ReadExact(pipe, respBuf, 0, respLen))
                        {
                            Log("Pipe EOF in response body. Exiting.");
                            return 0;
                        }
                        // 5. Preserve the native-messaging framing for the browser.
                        stdout.Write(header, 0, header.Length);
                        stdout.Write(respBuf, 0, respLen);
                        stdout.Flush();
                        Log(string.Format("Wrote {0} response bytes to stdout.", respLen));
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Proxy Exception: " + ex.ToString());
                return 0;
            }
        }
    }
}
