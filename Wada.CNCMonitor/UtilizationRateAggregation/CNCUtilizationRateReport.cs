namespace Wada.CNCMonitor.UtilizationRateAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class CNCUtilizationRateReport
    {
        public CNCUtilizationRateReport()
        {
            ID = Ulid.NewUlid();
            UtilizationTimes ??= new List<UtilizationTime>();
        }

        public CNCUtilizationRateReport(IEnumerable<UtilizationTime> utilizationTimes) : this()
        {
            UtilizationTimes = utilizationTimes ?? throw new ArgumentNullException(nameof(utilizationTimes));
        }

        public Ulid ID { get; }

        /// <summary>
        /// 稼働時間
        /// </summary>
        [IgnoreDuringEquals]
        public IEnumerable<UtilizationTime> UtilizationTimes { get; init; }
    }

    /// <summary>
    /// 稼働時間
    /// </summary>
    /// <param name="MachineName">設備名</param>
    /// <param name="UtilizationState">状態</param>
    /// <param name="UtilizationMinute">稼働時間</param>
    public record class UtilizationTime(string MachineName, string UtilizationState, int UtilizationMinute);
}
