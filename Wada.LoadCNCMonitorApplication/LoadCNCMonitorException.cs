using System.Runtime.Serialization;

namespace Wada.LoadCNCMonitorApplication
{
    [Serializable]
    internal class LoadCNCMonitorException : Exception
    {
        public LoadCNCMonitorException()
        {
        }

        public LoadCNCMonitorException(string? message) : base(message)
        {
        }

        public LoadCNCMonitorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected LoadCNCMonitorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}