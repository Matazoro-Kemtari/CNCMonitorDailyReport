namespace Wada.CNCMonitor.ApplicationConfigurationAggregation
{
    /// <summary>
    /// レポート設定
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="Factory">工場名</param>
    /// <param name="IPAddress">IPアドレス</param>
    /// <param name="MachineName">設備名</param>
    /// <param name="DisplayMachineName">表示名</param>
    /// <param name="ReportOutputOrder">出力順位</param>
    public record class CNCMonitorLog(string ID, string Factory, string IPAddress, string MachineName, string DisplayMachineName, int ReportOutputOrder);
}