using System.Net;

namespace Wada.CNCMonitor.CNCMonitorAggregation
{
    /// <summary>
    /// CNC稼働ログ
    /// </summary>
    /// <param name="PickedDate">取得日</param>
    /// <param name="Factory">工場名</param>
    /// <param name="IPAddress">IPアドレス</param>
    /// <param name="MachineName">設備名</param>
    /// <param name="CNCMonitorRecords">ログレコードリスト</param>
    public record class CNCMonitorByMachine(
        DateTime PickedDate,
        string Factory,
        IPAddress IPAddress,
        string MachineName,
        IEnumerable<CNCMonitorRecord> CNCMonitorRecords
        );

    /// <summary>
    /// CNC稼働レコード
    /// </summary>
    /// <param name="RecordTime">記録日時</param>
    /// <param name="State">状態</param>
    /// <param name="ProgramName">実行プログラム名</param>
    /// <param name="FeedRate">送り速度</param>
    /// <param name="SpindleRotation">回転数</param>
    /// <param name="RunMode">モード</param>
    /// <param name="RunState">自動運転状態</param>
    /// <param name="Emergency">緊急停止</param>
    public record class CNCMonitorRecord(
        DateTime RecordTime,
        string State,
        string ProgramName,
        int FeedRate,
        int SpindleRotation,
        int RunMode,
        int RunState,
        int Emergency);
}