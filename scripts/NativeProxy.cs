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
                var dir = @"C:\KeePassNatMsg-E2E";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "proxy.log"), $"[{DateTime.Now:HH:mm:ss.fff}] {msg}\r\n");
            }
            catch { }
        }

        static int Main(string[] args)
        {
            var pipeName = "keepassxc\\" + Environment.UserName + "\\kpxc_server";
            Log("Proxy started. Target pipe: " + pipeName);

            try
            {
                using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    Log("Connecting to named pipe...");
                    pipe.Connect(5000);
                    Log("Connected to pipe!");

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
                        Log($"Read header length: {length} bytes");

                        if (length <= 0 || length > 10 * 1024 * 1024)
                        {
                            Log($"Invalid length: {length}. Exiting.");
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
                        Log("Read full message body.");

                        // 3. Forward to named pipe
                        pipe.Write(body, 0, body.Length);
                        pipe.Flush();
                        Log("Forwarded message to pipe.");

                        // 4. Read response from named pipe
                        var respBuf = new byte[65536];
                        int respLen = pipe.Read(respBuf, 0, respBuf.Length);
                        Log($"Read {respLen} bytes from pipe.");
                        if (respLen <= 0)
                        {
                            Log("Zero bytes read from pipe. Exiting.");
                            return 0;
                        }

                        // 5. Forward response to stdout with 4-byte header
                        var respHeader = BitConverter.GetBytes(respLen);
                        stdout.Write(respHeader, 0, 4);
                        stdout.Write(respBuf, 0, respLen);
                        stdout.Flush();
                        Log("Wrote response to stdout.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Proxy Exception: {ex}");
                return 0;
            }
        }
    }
}
