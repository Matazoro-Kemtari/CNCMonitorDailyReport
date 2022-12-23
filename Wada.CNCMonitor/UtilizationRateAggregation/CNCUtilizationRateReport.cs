using Wada.CNCMonitor.ValueObjects;

namespace Wada.CNCMonitor.UtilizationRateAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class CNCUtilizationRateReport
    {
        public CNCUtilizationRateReport()
        {
            ID = Ulid.NewUlid();
            UtilizationAggregations ??= new List<UtilizationAggregation>();
        }

        public CNCUtilizationRateReport(DateTime aggregateDate, IEnumerable<UtilizationAggregation> utilizationTimes) : this()
        {
            AggregateDate = aggregateDate;
            UtilizationAggregations = utilizationTimes ?? throw new ArgumentNullException(nameof(utilizationTimes));
        }

        public Ulid ID { get; }

        /// <summary>
        /// 集計日
        /// </summary>
        [IgnoreDuringEquals]
        public DateTime AggregateDate { get; init; }

        /// <summary>
        /// 稼働時間
        /// </summary>
        [IgnoreDuringEquals]
        public IEnumerable<UtilizationAggregation> UtilizationAggregations { get; init; }
    }

    /// <summary>
    /// 稼働時間
    /// </summary>
    /// <param name="Fctory">工場名</param>
    /// <param name="MachineName">設備名</param>
    /// <param name="OutputOrder">表示順位</param>
    /// <param name="UtilizationTimes">稼働時間</param>
    public record class UtilizationAggregation(string Fctory, string MachineName, int OutputOrder, IEnumerable<UtilizationTime> UtilizationTimes);

    /// <summary>
    /// 稼働時間
    /// </summary>
    /// <param name="HistogramColor">グラフ色</param>
    /// <param name="UtilizationMinute">稼働時間</param>
    public record class UtilizationTime(HistogramColor HistogramColor, int UtilizationMinute);
}
