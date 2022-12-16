using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using System.Net;
using System.Text;
using Wada.CNCMonitor;
using Wada.CNCMonitor.CNCMonitorAggregation;

namespace Wada.CNCMonitoredCSV.Tests
{
    [TestClass()]
    public class CNCMonitoredCSVTests
    {
        [TestMethod()]
        public async Task 正常系_CNC稼働ログが読み込めること()
        {
            // given
            Mock<ILogger> mock_logger = new();

            string file = MakeTestLog(normalLogs);

            // when
            using StreamReader reader = new(file, Encoding.GetEncoding("shift_jis"));
            LoadMachineLogsRecord loadMachineLogsRecord = new(reader, "Test工場", IPAddress.Parse("192.168.1.1"));
            ICNCMonitorLoader monitorLoader = new CNCMonitoredCSV(mock_logger.Object);
            CNCMonitorByMachine actual = await monitorLoader.LoadMachineLogsAsync(loadMachineLogsRecord);

            // then
            Assert.IsNotNull(actual);
            Assert.AreEqual(new DateTime(2022, 10, 25), actual.PickedDate);
            Assert.AreEqual("RC-1号機", actual.MachineName);
            Assert.AreEqual(IPAddress.Parse("192.168.1.1"), actual.IPAddress);
            Assert.AreEqual(32, actual.CNCMonitorRecords.Count());
        }

        [TestMethod]
        public async Task 異常系_ファイルが空の場合例外を返すこと()
        {
            // given
            Mock<ILogger> mock_logger = new();
            string file = MakeTestLog("");

            // when
            using StreamReader reader = new(file, Encoding.GetEncoding("shift_jis"));
            LoadMachineLogsRecord loadMachineLogsRecord = new(reader, "Test工場", IPAddress.Parse("192.168.1.1"));
            ICNCMonitorLoader monitorLoader = new CNCMonitoredCSV(mock_logger.Object);


            async Task target()
            {
                _ = await monitorLoader.LoadMachineLogsAsync(loadMachineLogsRecord);
            }

            // then
            var msg = "CNC稼働設備ログが記録されていません";
            var ex = await Assert.ThrowsExceptionAsync<CNCMonitorLoaderException>(target);
            Assert.AreEqual(msg, ex.Message);
        }

        public static string MakeTestLog(string text)
        {
            string file = @"TestMonitor.csv";
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using StreamWriter writer = new(file, false, Encoding.GetEncoding("shift_jis"));
            writer.Write(text);
            writer.Close();
            return file;
        }

        public static readonly string normalLogs =
  @"日時,2022年10月25日
機械名,RC-1号機,192.168.1.1
Date,State,ProgName,mdata,data,F,S,auto,run,motion,mstb,emergency,alarm,edit,M,T,SLM,NCComment
2022/10/25 00:00,未接続（電源OFF)
2022/10/25 00:01,未接続（電源OFF)
2022/10/25 00:02,未接続（電源OFF)
2022/10/25 00:03,未接続（電源OFF)
2022/10/25 00:04,未接続（電源OFF)
2022/10/25 08:26,接続（電源ON),O12,12,12,0,0,5,0,0,0,1,0,0,00,00,0,
2022/10/25 08:27,接続（電源ON),O12,12,12,10000,0,5,0,0,0,0,0,0,00,00,0,
2022/10/25 08:28,接続（電源ON),O12,12,12,0,0,5,0,0,0,0,0,0,00,00,0,
2022/10/25 08:29,接続（電源ON),O12,12,12,0,0,5,0,0,0,0,0,0,00,00,0,
2022/10/25 08:30,接続（電源ON),O12,12,12,0,0,5,0,0,0,0,0,0,00,00,0,
2022/10/25 08:51,接続（電源ON),O12,12,12,0,601,5,1,0,0,0,0,0,19,01,0,
2022/10/25 08:52,接続（電源ON),O12,12,12,107,601,5,1,0,0,0,0,0,19,01,0,
2022/10/25 08:53,接続（電源ON),O9002,12,9002,0,0,0,3,0,1,0,0,23,06,30,0,
2022/10/25 08:54,接続（電源ON),O12,12,12,0,0,5,1,0,0,0,0,0,19,30,0,
2022/10/25 08:55,接続（電源ON),O9002,12,9002,0,0,5,1,0,0,0,0,0,19,30,0,
2022/10/25 08:56,接続（電源ON),O9001,12,9001,0,0,1,3,0,1,0,0,23,06,17,0,(ATC-SUB 6071-6),
2022/10/25 08:57,接続（電源ON),O6838,12,6838,799,3501,1,3,1,0,0,0,23,08,17,0,(22T-0180/-0181 OSAE NC2),(D10PRISM GAIKEI START),(STARTPOINT X0. Y0. Z100.),(LASTPOINT Z100.),(TIME 2 MIN ),
2022/10/25 08:58,接続（電源ON),O12,12,12,10000,0,5,0,0,0,0,0,0,09,17,0,
2022/10/25 08:59,接続（電源ON),O12,12,12,0,0,1,3,0,1,0,0,23,01,17,0,
2022/10/25 09:00,接続（電源ON),O6840,12,6840,8000,3501,1,3,1,0,0,0,23,08,17,0,(22T-0180/-0181 OSAE NC2),(D10PRISM SHIAGE GAIKEI-2),(STARTPOINT X0. Y0. Z100.),(LASTPOINT Z100.),(TIME 1 MIN ),
2022/10/25 09:01,接続（電源ON),O12,12,12,0,0,1,0,0,0,0,0,0,09,17,0,
2022/10/25 09:02,接続（電源ON),O12,12,12,10935,0,1,3,1,0,0,0,23,19,17,0,
2022/10/25 09:03,接続（電源ON),O6839,12,6839,10046,3501,1,3,1,0,0,0,23,08,17,0,(22T-0180/-0181 OSAE NC2),(D10PRISM SHIAGE GAIKEI-1),(STARTPOINT X0. Y0. Z100.),(LASTPOINT Z100.),(TIME 1 MIN ),
2022/10/25 09:04,接続（電源ON),O12,12,12,8000,0,5,0,0,0,0,0,0,09,17,0,
2022/10/25 09:05,接続（電源ON),O9001,12,9001,10000,0,1,3,1,0,0,0,23,19,17,0,(ATC-SUB 6071-6),
2022/10/25 09:06,接続（電源ON),O6841,12,6841,8000,3501,1,3,1,0,0,0,23,08,17,0,(22T-0180/-0181 OSAE NC2),(D10PRISM BUNKATU),(STARTPOINT X0. Y0. Z100.),(LASTPOINT Z100.),(TIME 1 MIN ),
2022/10/25 09:07,接続（電源ON),O6841,12,6841,800,3501,1,3,1,0,0,0,23,08,17,0,(22T-0180/-0181 OSAE NC2),(D10PRISM BUNKATU),(STARTPOINT X0. Y0. Z100.),(LASTPOINT Z100.),(TIME 1 MIN ),
2022/10/25 16:56,未接続（電源OFF)
2022/10/25 16:57,未接続（電源OFF)
2022/10/25 16:58,未接続（電源OFF)
2022/10/25 16:59,未接続（電源OFF)
2022/10/25 17:00,未接続（電源OFF)
";
    }
}