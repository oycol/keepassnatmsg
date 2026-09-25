using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace KeePassNatMsg.Protocol.Listener
{
    public sealed class NamedPipeListener : IListener
    {
        private const int MaxMessageSize = 10 * 1024 * 1024;
        private const int Threads = 5;
        private readonly string _name;
        private volatile bool _active;

        // _threads is accessed from multiple threads (Run, ThreadClosed, Write, Stop).
        // All accesses must be inside lock(_threads).
        private readonly List<PipeThreadState> _threads;

        public event EventHandler<PipeMessageReceivedEventArgs> MessageReceived;

        public NamedPipeListener(string name)
        {
            _name = name;
            _threads = new List<PipeThreadState>();
        }

        public void Start()
        {
            _active = true;
            for (var i = 0; i < Threads; i++)
            {
                CreateAndRunThread();
            }
        }

        public void Stop()
        {
            _active = false;
            List<PipeThreadState> snapshot;
            lock (_threads)
            {
                snapshot = new List<PipeThreadState>(_threads);
                _threads.Clear();
            }
            foreach (var pts in snapshot)
            {
                pts.Close(false);
            }
        }

        public void Write(string msg)
        {
            PipeThreadState pts = null;
            lock (_threads)
            {
                pts = _threads.Find(x => x.Server != null && x.Server.IsConnected);
            }
            if (pts != null)
            {
                var pw = new PipeWriter(pts.Server);
                pw.Send(msg);
            }
        }

        private void CreateAndRunThread()
        {
            var t = new Thread(Run) { IsBackground = true };
            PipeThreadState pts;
            lock (_threads)
            {
                pts = new PipeThreadState(t);
                _threads.Add(pts);
            }
            t.Start(pts);
        }

        private void RunThreadClosed(object args)
        {
            var oldPts = (PipeThreadState)args;
            lock (_threads)
            {
                _threads.Remove(oldPts);
            }
            oldPts.Close(false);

            if (!_active) return;

            // Spawn a fresh replacement thread to keep the pool full.
            var t = new Thread(Run) { IsBackground = true };
            PipeThreadState newPts;
            lock (_threads)
            {
                if (!_active) return;
                newPts = new PipeThreadState(t);
                _threads.Add(newPts);
            }
            t.Start(newPts);
        }

        private void ThreadClosed(PipeThreadState pts)
        {
            if (_active)
            {
                var t = new Thread(RunThreadClosed);
                t.Start(pts);
            }
        }

        private static bool ReadExact(Stream stream, byte[] buffer, int offset, int count, bool allowEof)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read == 0)
                {
                    if (allowEof && offset == 0) return false;
                    throw new EndOfStreamException("Truncated pipe request");
                }
                offset += read;
                count -= read;
            }
            return true;
        }

        private void Run(object args)
        {
            var pts = (PipeThreadState)args;

            // Use None (synchronous) instead of Asynchronous to avoid mixing sync
            // reads on an async-flagged pipe, which can cause ObjectDisposedException
            // on Windows 11 64-bit and is generally undefined behaviour.
            var server = new NamedPipeServerStream(
                _name,
                PipeDirection.InOut,
                Threads,
                PipeTransmissionMode.Byte,
                PipeOptions.None);

            lock (_threads)
            {
                pts.Server = server;
            }

            try
            {
                server.WaitForConnection();

                while (_active && server.IsConnected)
                {
                    // The pipe is in byte mode: one Read may contain only part of a
                    // header/body. Read the exact framed request before dispatch.
                    var header = new byte[4];
                    if (!ReadExact(server, header, 0, header.Length, true)) break;
                    int length = header[0] | (header[1] << 8) |
                        (header[2] << 16) | (header[3] << 24);
                    if (length <= 0 || length > MaxMessageSize)
                        throw new IOException("Invalid pipe request length: " + length);
                    var data = new byte[length];
                    ReadExact(server, data, 0, length, false);
                    var handler = MessageReceived;
                    if (handler != null)
                        handler(this, new PipeMessageReceivedEventArgs(new PipeWriter(server), data));
                }
            }
            catch (IOException)
            {
                // Client disconnected abruptly — normal operating condition.
            }
            catch (ObjectDisposedException)
            {
                // Stop() closed a waiting or connected pipe.
            }

            ThreadClosed(pts);
        }
    }

    public class PipeMessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public IMessageWriter Writer { get; set; }

        public PipeMessageReceivedEventArgs(IMessageWriter writer, byte[] data)
        {
            Writer = writer;
            Message = System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
