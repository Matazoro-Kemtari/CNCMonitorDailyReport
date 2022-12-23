using System.Net;

namespace Wada.CNCMonitor.CNCMonitorAggregation
{
    /// <summary>
    /// CNC稼働ログ
    /// </summary>
    public record class CNCMonitorByMachine
    {
        private CNCMonitorByMachine(Ulid iD, DateTime pickedDate, string factory, IPAddress iPAddress, string machineName, IEnumerable<CNCMonitorRecord> cNCMonitorRecords)
        {
            ID = iD;
            PickedDate = pickedDate;
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            IPAddress = iPAddress ?? throw new ArgumentNullException(nameof(iPAddress));
            MachineName = machineName ?? throw new ArgumentNullException(nameof(machineName));
            CNCMonitorRecords = cNCMonitorRecords ?? throw new ArgumentNullException(nameof(cNCMonitorRecords));
        }

        public CNCMonitorByMachine(DateTime pickedDate, string factory, IPAddress iPAddress, string machineName, IEnumerable<CNCMonitorRecord> cNCMonitorRecords)
        {
            ID = Ulid.NewUlid();
            PickedDate = pickedDate;
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            IPAddress = iPAddress ?? throw new ArgumentNullException(nameof(iPAddress));
            MachineName = machineName ?? throw new ArgumentNullException(nameof(machineName));
            CNCMonitorRecords = cNCMonitorRecords ?? throw new ArgumentNullException(nameof(cNCMonitorRecords));
        }

        /// <summary>
        /// DBなどの値からインスタンスを再構成
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pickedDate"></param>
        /// <param name="factory"></param>
        /// <param name="iPAddress"></param>
        /// <param name="machineName"></param>
        /// <param name="cNCMonitorRecords"></param>
        /// <returns></returns>
        public static CNCMonitorByMachine ReConstruct(Ulid id, DateTime pickedDate, string factory, IPAddress iPAddress, string machineName, IEnumerable<CNCMonitorRecord> cNCMonitorRecords) =>
            new(id, pickedDate, factory, iPAddress, machineName, cNCMonitorRecords);

        public Ulid ID { get; init; }

        /// <summary>
        /// 取得日
        /// </summary>
        public DateTime PickedDate { get; init; }

        /// <summary>
        /// 工場名
        /// </summary>
        public string Factory { get; init; }

        /// <summary>
        /// IPアドレス
        /// </summary>
        public IPAddress IPAddress { get; init; }

        /// <summary>
        /// 設備名
        /// </summary>
        public string MachineName { get; init; }

        /// <summary>
        /// ログレコードリスト
        /// </summary>
        public IEnumerable<CNCMonitorRecord> CNCMonitorRecords { get; init; }
    }

    /// <summary>
    /// CNC稼働レコード
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class CNCMonitorRecord
    {
        public CNCMonitorRecord(DateTime recordTime, string state, string programName, int feedRate, int spindleRotation, int runMode, int runState, int emergency)
        {
            RecordTime = recordTime;
            ConnectionState = state ?? throw new ArgumentNullException(nameof(state));
            ProgramName = programName ?? throw new ArgumentNullException(nameof(programName));
            FeedRate = feedRate;
            SpindleRotation = spindleRotation;
            RunMode = (RunMode)runMode;
            RunState = (AutonomousWorkState)runState;
            Emergency = (EmergencyState)emergency;
        }

        /// <summary>
        /// 記録日時
        /// </summary>
        public DateTime RecordTime { get; init; }

        /// <summary>
        /// 接続状態
        /// </summary>
        [IgnoreDuringEquals]
        public string ConnectionState { get; init; }

        /// <summary>
        /// 実行プログラム名
        /// </summary>
        [IgnoreDuringEquals]
        public string ProgramName { get; init; }

        /// <summary>
        /// 送り速度
        /// </summary>
        [IgnoreDuringEquals]
        public int FeedRate { get; init; }

        /// <summary>
        /// 回転数
        /// </summary>
        [IgnoreDuringEquals]
        public int SpindleRotation { get; init; }

        /// <summary>
        /// モード
        /// </summary>
        [IgnoreDuringEquals]
        public RunMode RunMode { get; init; }

        /// <summary>
        /// 自動運転状態
        /// </summary>
        [IgnoreDuringEquals]
        public AutonomousWorkState RunState { get; init; }

        /// <summary>
        /// 緊急停止
        /// </summary>
        [IgnoreDuringEquals]
        public EmergencyState Emergency { get; init; }
    }

    public enum RunMode
    {
        // MDI運転
        MDIWork,
        // メモリ運転
        MemoryWork,
        // ****(定義外)
        Undefined,
        // EDIT
        Edit,
        // 手動HANDLE送り
        ManuallyHandleFeed,
        // JOG送り
        JogFeed,
        // TEACH IN JOG
        TeachJog,
        // TEACH IN HANDLE
        TeachHandle,
        // 手動インクリメンタル送り
        ManuallyIncrementalFeed,
        // 手動リファレンス点復帰
        ManuallyOriginMove,
        // DNC, スケジュール運転
        ScheduleWork,
        // CNCLog欠落
        MissingRecord = int.MaxValue,
    }

    public enum AutonomousWorkState
    {
        // リセット
        Reset,
        // 自動運転停止
        Stop,
        // 自動運転休止
        Suspend,
        // 自動運転起動
        Start,
        // CNCLog欠落
        MissingRecord = int.MaxValue,
    }

    public enum EmergencyState
    {
        // 正常
        Normal,
        // 非常停止
        Stop,
        // リセット
        Reset,
        // CNCLog欠落
        MissingRecord = int.MaxValue,
    }

    public class TestCNCMonitorByMachineFactory
    {
        private static readonly DateTime _defaultPickedDate = DateTime.Parse("2022/5/5");

        public static CNCMonitorByMachine Create(
            Ulid? id = null,
            DateTime? pickedDate = null,
            string factory = "A工場",
            IPAddress? ipAddress = null,
            string machineName = "設備1号",
            IEnumerable<CNCMonitorRecord>? cncMonitorRecords = null)
        {
            pickedDate ??= _defaultPickedDate;
            ipAddress ??= IPAddress.Parse("192.168.1.1");
            cncMonitorRecords ??= new CNCMonitorRecord[]
            {
                TestCNCMonitorRecordFactory.Create(),
            };

            if (id == null)
                return new(pickedDate.Value, factory, ipAddress, machineName, cncMonitorRecords);
            else
                return CNCMonitorByMachine.ReConstruct(id.Value, pickedDate.Value, factory, ipAddress, machineName, cncMonitorRecords);
        }

        class TestCNCMonitorRecordFactory
        {
            public static CNCMonitorRecord Create(
                DateTime? recordTime = null,
                string state = "接続（電源ON)",
                string programName = "O12",
                int feedRate = 5,
                int spindleRotation = 10,
                int runMode = 1,
                int runState = 2,
                int emergency = 0)
            {
                recordTime ??= _defaultPickedDate;
                return new(recordTime.Value, state, programName, feedRate, spindleRotation, runMode, runState, emergency);
            }
        }
    }
}