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
            Close(false);
        }

        public void Close(bool waitForExit)
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

            if (waitForExit)
            {
                try
                {
                    if (Thread != null && Thread.IsAlive && Thread != Thread.CurrentThread)
                    {
                        Thread.Join(50);
                    }
                }
                catch { }
            }
        }
    }
}
