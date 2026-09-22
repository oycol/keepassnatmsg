using System;
using System.IO;
using System.IO.Pipes;

namespace KeePassNatMsgProxy
{
    class Program
    {
        static int Main(string[] args)
        {
            var pipeName = "keepassxc\\" + Environment.UserName + "\\kpxc_server";
            try
            {
                using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    pipe.Connect(5000);
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
                            if (r <= 0) return 0; // Stdin closed by browser
                            read += r;
                        }
                        int length = BitConverter.ToInt32(header, 0);
                        if (length <= 0 || length > 10 * 1024 * 1024) return 0;

                        // 2. Read message body
                        var body = new byte[length];
                        read = 0;
                        while (read < length)
                        {
                            int r = stdin.Read(body, read, length - read);
                            if (r <= 0) return 0;
                            read += r;
                        }

                        // 3. Forward to named pipe
                        pipe.Write(body, 0, body.Length);
                        pipe.Flush();

                        // 4. Read response from named pipe
                        var respBuf = new byte[65536];
                        int respLen = pipe.Read(respBuf, 0, respBuf.Length);
                        if (respLen <= 0) return 0;

                        // 5. Forward response to stdout with 4-byte header
                        var respHeader = BitConverter.GetBytes(respLen);
                        stdout.Write(respHeader, 0, 4);
                        stdout.Write(respBuf, 0, respLen);
                        stdout.Flush();
                    }
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
