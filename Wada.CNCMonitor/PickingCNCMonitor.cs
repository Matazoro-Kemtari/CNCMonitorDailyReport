using System.Net;

namespace Wada.CNCMonitor
{
    /// <summary>
    /// CNC稼働ログファイル情報
    /// </summary>
    /// <param name="PickingDate">取得日</param>
    /// <param name="Factory">工場名</param>
    /// <param name="IPAddress">IPアドレス</param>
    /// <param name="MachineName">設備名</param>
    /// <param name="FilePath">ファイルパス</param>
    public record class PickingCNCMonitor(DateTime PickingDate, string Factory, IPAddress IPAddress, string MachineName, string FilePath);
}
