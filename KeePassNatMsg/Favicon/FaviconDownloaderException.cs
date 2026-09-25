using System;

namespace KeePassNatMsg.Favicon
{
    public enum FaviconErrorStatus
    {
        NotFound,
        NetworkError,
        InvalidData,
        Cancelled
    }

    public sealed class FaviconDownloaderException : Exception
    {
        public FaviconErrorStatus Status { get; private set; }

        public FaviconDownloaderException(FaviconErrorStatus status)
            : base(string.Format("Favicon error: {0}", status))
        {
            Status = status;
        }

        public FaviconDownloaderException(FaviconErrorStatus status, Exception innerException)
            : base(string.Format("Favicon error: {0}", status), innerException)
        {
            Status = status;
        }
    }
}
