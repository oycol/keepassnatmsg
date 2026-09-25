using System.IO.Pipes;

namespace KeePassNatMsg.Protocol.Listener
{
    public class PipeWriter : IMessageWriter
    {
        private readonly NamedPipeServerStream _server;
        private readonly System.Text.UTF8Encoding _utf8;

        public PipeWriter(NamedPipeServerStream server)
        {
            _server = server;
            _utf8 = new System.Text.UTF8Encoding(false);
        }

        public void Send(string msg)
        {
            var data = _utf8.GetBytes(msg);
            if (data.Length == 0 || data.Length > 10 * 1024 * 1024)
                throw new System.IO.InvalidDataException("Invalid pipe response length: " + data.Length);
            var header = new byte[4];
            header[0] = (byte)data.Length;
            header[1] = (byte)(data.Length >> 8);
            header[2] = (byte)(data.Length >> 16);
            header[3] = (byte)(data.Length >> 24);
            // Keep response bytes together with their prefix even when broadcasts
            // and request handlers write concurrently to the same pipe.
            lock (_server)
            {
                _server.Write(header, 0, header.Length);
                _server.Write(data, 0, data.Length);
                _server.Flush();
            }
        }
    }
}
