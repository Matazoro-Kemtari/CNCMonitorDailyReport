using System.Net;
using Wada.CNCMonitor.CNCMonitorAggregation;

namespace Wada.CNCMonitor
{
    public interface ICNCMonitorLoader
    {
        /// <summary>
        /// CSVを読み込む
        /// </summary>
        /// <param name="loadMachineLogsRecord"></param>
        /// <returns></returns>
        public Task<CNCMonitorByMachine> LoadMachineLogsAsync(LoadMachineLogsRecord loadMachineLogsRecord);
    }

    /// <summary>
    /// LoadMachineLogsAsync引数用データレコード
    /// </summary>
    /// <param name="Reader"></param>
    /// <param name="Factory"></param>
    /// <param name="IPAddress"></param>
    public record class LoadMachineLogsRecord(StreamReader Reader, string Factory, IPAddress IPAddress);
}
