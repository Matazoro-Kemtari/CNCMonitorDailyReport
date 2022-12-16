using System.Net;

namespace Wada.CNCMonitor.CNCMonitorAggregation
{
    /// <summary>
    /// CNC稼働ログファイル情報
    /// </summary>
    /// <param name="PickingDate">取得日</param>
    /// <param name="Factory">工場名</param>
    /// <param name="IPAddress">IPアドレス</param>
    /// <param name="MachineName">設備名</param>
    public record class PickingCNCMonitor(DateTime PickingDate,
                                          string Factory,
                                          IPAddress IPAddress,
                                          string MachineName)
    {
        /// <summary>
        /// ファイルパス
        /// </summary>
        public string GetFilePath(string baseDirectory) =>
            Path.Combine(
                baseDirectory,
                Factory,
                $"[{IPAddress}]_{PickingDate:yyyyMMdd}.csv");
    }

}
