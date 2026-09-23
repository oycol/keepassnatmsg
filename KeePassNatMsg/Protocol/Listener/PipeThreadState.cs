using System.IO.Pipes;
using System.Threading;

namespace KeePassNatMsg.Protocol.Listener
{
    public class PipeThreadState
    {
        public Thread Thread { get; private set; }
        public NamedPipeServerStream Server { get; set; }

        public PipeThreadState(Thread t)
        {
            Thread = t;
        }

        public void Close()
        {
            try
            {
                if (Server != null)
                {
                    Server.Close();
                    Server.Dispose();
                }
            }
            catch { }

            // Do not block indefinitely on Thread.Join() while shutting down,
            // which risks deadlock if the thread is completing an event dispatch.
            try
            {
                if (Thread != null && Thread.IsAlive && Thread != System.Threading.Thread.CurrentThread)
                {
                    Thread.Join(500);
                }
            }
            catch { }
        }
    }
}
