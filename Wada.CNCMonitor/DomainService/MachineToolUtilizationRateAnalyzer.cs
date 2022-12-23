using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Xml.Serialization;
using Wada.AOP.Logging;
using Wada.CNCMonitor.ApplicationConfigurationAggregation;
using Wada.CNCMonitor.CNCMonitorAggregation;
using Wada.CNCMonitor.UtilizationRateAggregation;
using Wada.CNCMonitor.ValueObjects;

namespace Wada.CNCMonitor.DomainService
{
    public interface IMachineToolUtilizationRateAnalyzer
    {
        /// <summary>
        /// 稼働状況分析
        /// </summary>
        /// <param name="cncMonitorByMachine"></param>
        CNCUtilizationRateReport Analyze(IEnumerable<CNCMonitorByMachine> cncMonitorByMachines);
    }

    public class MachineToolUtilizationRateAnalyzer : IMachineToolUtilizationRateAnalyzer
    {
        private readonly IConfiguration _configuration;

        public MachineToolUtilizationRateAnalyzer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Logging]
        public CNCUtilizationRateReport Analyze(IEnumerable<CNCMonitorByMachine> cncMonitorByMachines)
        {
            if (cncMonitorByMachines == null)
                throw new ArgumentNullException(nameof(cncMonitorByMachines));

            if (cncMonitorByMachines.GroupBy(x => x.PickedDate).Count() > 1)
                throw new CNCMonitorLoaderException("稼働ログの分析に複数の取得日は指定できません");

            // 測定抜けを穴埋めする


            // レポート設定を取得
            IEnumerable<CNCMonitorLog>? cncMonitorLogs = _configuration.GetSection("cncMonitorLogs").Get<CNCMonitorLog[]>();
            if (cncMonitorLogs == null)
                throw new CNCMonitorLoaderException("アプリケーション設定が読み込めませんでした");

            IEnumerable<UtilizationAggregation> UtilizationAggregations = cncMonitorByMachines
                .Select(x =>
                {
                    // 欠落したレコードを補う
                    var compensatedCNCMonitor = CompensateRecordMissing(x);

                    // TODO: 条件の見直し この後の流れも見直し

                    // TODO: DataAggregationRunning をコピーすれば良いが、Linq Selectも違うかも
                    CNCMonitorRecord? prevRecord = null;
                    int FSZeroCount = default;

                    // 停止判定閾値

                    const int StopJudgeThresholdValue = 10;
                    // 回転・送り速度総和加算
                    List<int> feeds = new(), spins = new();
                    TimeSpan elapsedTime = default;

                    IEnumerable<UtilizationTime?> _bufTimes = x.CNCMonitorRecords
                        .Select((current, index) =>
                        {
                            // 出力条件を確認する
                            var (recordMissing, intervalDiff, paramChanged, moveStoped, moveStarted) = MakeOutputCriteria(current, prevRecord, FSZeroCount, StopJudgeThresholdValue, feeds, spins);
                            IEnumerable<bool> _outputReady = new List<bool>
                            {
                                recordMissing,
                                paramChanged,
                                moveStoped,
                                moveStarted,
                            };
                            if (_outputReady.Any())
                            {
                                // F,S停止区間の初期化
                                FSZeroCount = moveStoped ? StopJudgeThresholdValue : default;
                                // 送り・回転数の初期化
                                feeds.Clear();
                                spins.Clear();

                                if (recordMissing)// これがアカン理由 記録抜けが合った場合の穴埋めだけど、下のnewしたのも返さないといけない
                                    return new(HistogramColor.Undefined, intervalDiff.Minutes);

                                var color = DetermineHistogramColor(current.RunMode, current.RunState, current.FeedRate, current.SpindleRotation);
                                return new(color, elapsedTime.Minutes);
                            }
                            else
                            {
                                elapsedTime.Add(new(0, 1, 0));

                                // F,Sの変化量がない連続区間をインクリメント
                                if (current.FeedRate == 0 && current.SpindleRotation == 0)
                                    FSZeroCount++;
                                else
                                    FSZeroCount = default;

                                // 回転・送り速度追加
                                feeds.Add(current.FeedRate);
                                spins.Add(current.SpindleRotation);

                                return null;
                            }
                        });
                    IEnumerable<UtilizationTime> utilizationTimes = (IEnumerable<UtilizationTime>)_bufTimes
                        .Where(x => x != null);

                    int outputOrder = cncMonitorLogs
                        .Where(y => y.Factory == x.Factory)
                        .Where(y => y.MachineName == x.MachineName)
                        .Select(y => y.ReportOutputOrder)
                        .SingleOrDefault();
                    if (outputOrder == 0)
                        outputOrder = int.MaxValue;
                    return new UtilizationAggregation(
                        x.Factory,
                        x.MachineName,
                        outputOrder,
                        utilizationTimes);
                });


            return new(cncMonitorByMachines.First().PickedDate.Date, UtilizationAggregations);

            /// <summary>欠落したレコードを補う</summary>
            static CNCMonitorByMachine CompensateRecordMissing(CNCMonitorByMachine cncMonitor)
            {
                List<CNCMonitorRecord> compensatedRecords = new();
                CNCMonitorRecord? prevRecord = null;
                cncMonitor.CNCMonitorRecords
                    .ToList()
                    .ForEach(x =>
                    {
                        // 前ログとの時間差を確認
                        if (prevRecord == null)
                        {
                            compensate(0,
                                       (int)x.RecordTime.TimeOfDay.TotalSeconds,
                                       x.RecordTime,
                                       compensatedRecords);
                            prevRecord = x;
                        }
                        else
                        {
                            TimeSpan intervalDiff = x.RecordTime - prevRecord.RecordTime;
                            if (intervalDiff.TotalMinutes > 1)
                            {
                                compensate((int)prevRecord.RecordTime.TimeOfDay.TotalMinutes + 1,
                                           (int)x.RecordTime.TimeOfDay.TotalMinutes,
                                           x.RecordTime,
                                           compensatedRecords);
                            }
                        }
                        compensatedRecords.Add(x);
                    });

                // 1日の最後までログが無ければ追加
                DateTime lastRecordDay = cncMonitor.CNCMonitorRecords.Select(x => x.RecordTime).Max();
                if (lastRecordDay.TimeOfDay < new TimeSpan(23, 59, 0))
                {
                    compensate((int)lastRecordDay.TimeOfDay.TotalMinutes + 1,
                               (int)new TimeSpan(23, 59, 0).TotalMinutes,
                               lastRecordDay,
                               compensatedRecords);
                }
                return cncMonitor with { CNCMonitorRecords = compensatedRecords };

                static void compensate(int minMinute, int maxMinute, DateTime recordTime, List<CNCMonitorRecord> compensatedRecords)
                {
                    for (int min = minMinute; min < maxMinute; min++)
                    {
                        // minMinute から不足分を補う
                        compensatedRecords.Add(
                            new CNCMonitorRecord(
                                recordTime.Date + new TimeSpan(0, min, 9),
                                string.Empty,
                                string.Empty,
                                default,
                                default,
                                (int)RunMode.MissingRecord,
                                (int)AutonomousWorkState.MissingRecord,
                                (int)EmergencyState.MissingRecord));
                    }
                }
            }

            static (bool recordMissing, TimeSpan intervalDiff, bool paramChanged, bool moveStoped, bool moveStarted) MakeOutputCriteria(CNCMonitorRecord current, CNCMonitorRecord? prevRecord, int FSZeroCount, int StopJudgeThresholdValue, List<int> feeds, List<int> spins)
            {
                // 前ログとの時間差を確認
                TimeSpan intervalDiff;
                if (prevRecord == null)
                    intervalDiff = new(0, 1, 0);
                else
                    intervalDiff = current.RecordTime - prevRecord.RecordTime;
                // 行間の経過時間が１分より大きいとき測定抜け
                bool recordMissing = intervalDiff.TotalMinutes > 1;

                // 加工機内部情報変更
                bool paramChanged =
                prevRecord?.ConnectionState != current.ConnectionState ||
                prevRecord?.ProgramName != current.ProgramName ||
                prevRecord?.RunMode != current.RunMode ||
                prevRecord?.RunState != current.RunState ||
                prevRecord?.Emergency != current.Emergency;

                // 移動・回転をしていたが一定期間停止したとき
                bool moveStoped = (FSZeroCount >= StopJudgeThresholdValue)
                    && (feeds.Max() > 0 || spins.Max() > 0);

                // 停止をしていたが動作が再開されるとき
                bool moveStarted = (FSZeroCount >= StopJudgeThresholdValue)
                    && (current.FeedRate > 0 || current.SpindleRotation > 0);

                return (recordMissing, intervalDiff, paramChanged, moveStoped, moveStarted);
            }
        }

        /// <summary>
        /// グラフの色を決定する
        /// </summary>
        /// <param name="runMode"></param>
        /// <param name="runState"></param>
        /// <param name="feedRate"></param>
        /// <param name="spindleRotation"></param>
        /// <returns></returns>
        private static HistogramColor DetermineHistogramColor(RunMode runMode, AutonomousWorkState runState, int feedRate, int spindleRotation)
        {
            switch (runMode)
            {
                case RunMode.MemoryWork when runState == AutonomousWorkState.Start
                                             && (feedRate == 10 || spindleRotation == 10):
                    return HistogramColor.AutonomousWork;
                case RunMode.MDIWork:
                case RunMode.ManuallyHandleFeed:
                case RunMode.JogFeed when feedRate <= 2000
                                          && (feedRate == 10 || spindleRotation == 10):
                    return HistogramColor.ManuallyWork;
                case RunMode.Edit:
                    return HistogramColor.Edit;
                default:
                    return HistogramColor.OriginStop;
            }
        }

    }
}

