using System.Runtime.Serialization;

namespace Wada.CNCMonitor
{
    [Serializable]
    public class CNCMonitorLoaderException : Exception
    {
        public CNCMonitorLoaderException()
        {
        }

        public CNCMonitorLoaderException(string? message) : base(message)
        {
        }

        public CNCMonitorLoaderException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CNCMonitorLoaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}