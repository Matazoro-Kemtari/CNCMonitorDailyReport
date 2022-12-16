using System.Text;
using Wada.AOP.Logging;
using Wada.CNCMonitor;

[module: Logging]
namespace Wada.CNCMonitoredCSV
{
    public class StreamOpener : IStreamOpener
    {
        [Logging]
        public StreamReader Open(string path) =>
            new(path, Encoding.GetEncoding("shift_jis"));
    }
}
