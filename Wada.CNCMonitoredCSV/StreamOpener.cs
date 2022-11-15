using NLog;
using System.Reflection;
using System.Text;
using Wada.CNCMonitor;

namespace Wada.CNCMonitoredCSV
{
    public class StreamOpener : IStreamOpener
    {
        private readonly ILogger logger;

        public StreamOpener(ILogger logger)
        {
            this.logger = logger;
        }

        public StreamReader Open(string path)
        {
            logger.Debug("Start {0}", MethodBase.GetCurrentMethod()?.Name);

            StreamReader reader = new(path, Encoding.GetEncoding("shift_jis"));

            logger.Debug("Finish {0}", MethodBase.GetCurrentMethod()?.Name);

            return reader;
        }
    }
}
