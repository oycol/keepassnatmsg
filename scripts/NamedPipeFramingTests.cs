// Standalone protocol regression: compile with mcs -langversion:5 -out:/tmp/pipe-framing.exe
// scripts/NamedPipeFramingTests.cs KeePassNatMsg/Protocol/Listener/{IListener,IMessageWriter,PipeThreadState,PipeWriter,NamedPipeListener}.cs
// Compile scripts/NativeProxy.cs separately to /tmp/native-proxy.exe, then run:
// mono /tmp/pipe-framing.exe /tmp/native-proxy.exe
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using KeePassNatMsg.Protocol.Listener;

class NamedPipeFramingTests
{
    static void Exact(Stream stream, byte[] data, int offset, int length)
    {
        while (length > 0)
        {
            int n = stream.Read(data, offset, length);
            if (n <= 0) throw new EndOfStreamException("Incomplete frame");
            offset += n;
            length -= n;
        }
    }

    static byte[] Frame(string message)
    {
        byte[] body = Encoding.UTF8.GetBytes(message);
        byte[] frame = new byte[4 + body.Length];
        frame[0] = (byte)body.Length;
        frame[1] = (byte)(body.Length >> 8);
        frame[2] = (byte)(body.Length >> 16);
        frame[3] = (byte)(body.Length >> 24);
        Buffer.BlockCopy(body, 0, frame, 4, body.Length);
        return frame;
    }

    static string Receive(Stream stream)
    {
        byte[] header = new byte[4];
        Exact(stream, header, 0, 4);
        int size = header[0] | header[1] << 8 | header[2] << 16 | header[3] << 24;
        if (size <= 0 || size > 10 * 1024 * 1024) throw new Exception("Invalid frame size " + size);
        byte[] body = new byte[size];
        Exact(stream, body, 0, size);
        return Encoding.UTF8.GetString(body);
    }

    static void Check(bool condition, string what)
    {
        if (!condition) throw new Exception(what);
    }

    static void SendInPieces(Stream stream, string text)
    {
        byte[] data = Frame(text);
        // Deliberately split the length, UTF-8 multibyte character, and body.
        int offset = 0;
        int[] chunks = { 1, 2, 1, 3, 2, data.Length };
        foreach (int chunk in chunks)
        {
            int count = Math.Min(chunk, data.Length - offset);
            if (count == 0) break;
            stream.Write(data, offset, count);
            stream.Flush();
            offset += count;
        }
    }

    static void DirectPipe()
    {
        string name = "framing-test-" + Guid.NewGuid().ToString("N");
        var listener = new NamedPipeListener(name);
        int count = 0;
        string large = "{\"response\":\"" + new string('x', 1100000) + "\"}";
        listener.MessageReceived += delegate(object sender, PipeMessageReceivedEventArgs e)
        {
            int index = Interlocked.Increment(ref count);
            string expected = index == 1 ? "{\"q\":\"雪\"}" : index == 2 ? "{\"q\":2}" : "{\"q\":\"" + new string('y', 1100000) + "\"}";
            Check(e.Message == expected, "Pipe request boundary #" + index);
            e.Writer.Send(index == 1 ? large : index == 2 ? "{\"ok\":2}" : "{\"ok\":3}");
        };
        listener.Start();
        try
        {
            using (var client = new NamedPipeClientStream(".", name, PipeDirection.InOut))
            {
                client.Connect(5000);
                // Send first frame, wait for response before next frame (native proxy sequence).
                SendInPieces(client, "{\"q\":\"雪\"}");
                Check(Receive(client) == large, "Large pipe response truncated");
                SendInPieces(client, "{\"q\":2}");
                Check(Receive(client) == "{\"ok\":2}", "Second pipe response boundary");
                SendInPieces(client, "{\"q\":\"" + new string('y', 1100000) + "\"}");
                Check(Receive(client) == "{\"ok\":3}", "Large pipe request truncated");
            }
            Check(count == 3, "Expected exactly three requests, got " + count);
        }
        finally { listener.Stop(); }
    }

    static void Proxy(string proxyExe)
    {
        string name = "keepassxc\\" + Environment.UserName + "\\kpxc_server";
        var listener = new NamedPipeListener(name);
        string large = "{\"items\":\"" + new string('z', 1100000) + "\"}";
        int count = 0;
        listener.MessageReceived += delegate(object sender, PipeMessageReceivedEventArgs e)
        {
            int index = Interlocked.Increment(ref count);
            string expected = index == 1 ? "{\"request\":1}" : index == 2 ? "{\"request\":2}" : "{\"request\":\"" + new string('r', 1100000) + "\"}";
            Check(e.Message == expected, "Proxy request boundary #" + index);
            e.Writer.Send(index == 1 ? large : index == 2 ? "{\"answer\":2}" : "{\"answer\":3}");
        };
        listener.Start();
        Process process = null;
        try
        {
            process = new Process();
            process.StartInfo.FileName = "mono";
            process.StartInfo.Arguments = "\"" + proxyExe + "\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            SendInPieces(process.StandardInput.BaseStream, "{\"request\":1}");
            Check(Receive(process.StandardOutput.BaseStream) == large, "Proxy large response truncated");
            SendInPieces(process.StandardInput.BaseStream, "{\"request\":2}");
            Check(Receive(process.StandardOutput.BaseStream) == "{\"answer\":2}", "Proxy subsequent response contaminated");
            SendInPieces(process.StandardInput.BaseStream, "{\"request\":\"" + new string('r', 1100000) + "\"}");
            Check(Receive(process.StandardOutput.BaseStream) == "{\"answer\":3}", "Proxy large request truncated");
            process.StandardInput.Close();
            Check(process.WaitForExit(5000), "Proxy did not exit on EOF");
            Check(count == 3, "Proxy expected three requests, got " + count);
        }
        finally
        {
            if (process != null && !process.HasExited) process.Kill();
            listener.Stop();
        }
    }

    static int Main(string[] args)
    {
        try
        {
            DirectPipe();
            Console.WriteLine("PASS direct pipe framing");
            Proxy(args[0]);
            Console.WriteLine("PASS browser proxy framing");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
