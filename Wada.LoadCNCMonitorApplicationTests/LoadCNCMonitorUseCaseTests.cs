using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using System.Text;
using Wada.CNCMonitor;
using Wada.CNCMonitor.CNCMonitorAggregation;

namespace Wada.LoadCNCMonitorApplication.Tests
{
    [TestClass()]
    public class LoadCNCMonitorUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_CNC稼働ログが読み込みUseCaseが実行されること()
        {
            // given
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true)
                .Build();

            // when
            DateTime processDate = new(2022, 5, 5);
            CNCMonitorByMachine expected = TestCNCMonitorByMachineFactory.Create();

            // Mock ストリーム
            // stringをstreamに変換
            StreamReader stream = new(new MemoryStream(Encoding.UTF8.GetBytes(normalLogs)));
            Mock<IStreamOpener> mock_stream = new();
            mock_stream.Setup(x => x.Open(It.IsAny<string>()))
                .Returns(stream);

            // Mock CNC稼働ログローダー
            Mock<ICNCMonitorLoader> mock_loader = new();
            mock_loader.Setup(x => x.LoadMachineLogsAsync(It.IsAny<LoadMachineLogsRecord>()))
                .ReturnsAsync(expected);

            ILoadCNCMonitorUseCase loadCNCMonitorUseCase =
                new LoadCNCMonitorUseCase(configuration,
                                          mock_stream.Object,
                                          mock_loader.Object);
            var actual = await loadCNCMonitorUseCase.ExecuteAsync(processDate);

            // then
            Assert.AreEqual(3, actual.Count());
            Assert.AreEqual(expected, actual.ToArray()[0]);
        }

        public static readonly string normalLogs =
  @"日時,2022年10月25日
機械名,RC-1号機,192.168.1.1
Date,State,ProgName,mdata,data,F,S,auto,run,motion,mstb,emergency,alarm,edit,M,T,SLM,NCComment
2022/10/25 00:00,未接続（電源OFF)
2022/10/25 08:26,接続（電源ON),O12,12,12,0,0,5,0,0,0,1,0,0,00,00,0,
";

        [TestMethod]
        public async Task 異常系_ログが無いとき例外を返すこと()
        {
            // given
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true)
                .Build();

            // when
            DateTime processDate = new(2022, 5, 5);

            // Mock ストリーム
            // stringをstreamに変換
            Mock<IStreamOpener> mock_stream = new();
            mock_stream.Setup(x => x.Open(It.IsAny<string>()))
                .Throws(new FileNotFoundException());

            // Mock CNC稼働ログローダー
            Mock<ICNCMonitorLoader> mock_loader = new();

            ILoadCNCMonitorUseCase loadCNCMonitorUseCase =
                new LoadCNCMonitorUseCase(configuration,
                                          mock_stream.Object,
                                          mock_loader.Object);
            async Task<IEnumerable<CNCMonitorByMachine>> target()
            {
                return await loadCNCMonitorUseCase.ExecuteAsync(processDate);
            }

            // then
            var msg = "CNC稼働ログ読み込みに失敗しました";
            var ex = await Assert.ThrowsExceptionAsync<LoadCNCMonitorException>(target);
            Assert.AreEqual(msg, ex.Message);
        }
    }
}