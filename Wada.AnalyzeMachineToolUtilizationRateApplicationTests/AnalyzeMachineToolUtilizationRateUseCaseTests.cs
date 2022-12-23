using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.CNCMonitor.CNCMonitorAggregation;
using Wada.CNCMonitor.DomainService;

namespace Wada.AnalyzeMachineToolUtilizationRateApplication.Tests
{
    [TestClass()]
    public class AnalyzeMachineToolUtilizationRateUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ユースケースが実行されるとリポジトリが実行されること()
        {
            // given
            Mock<IMachineToolUtilizationRateAnalyzer> mock_analyzer = new();

            // when
            CNCMonitorByMachine[] cncMonitorByMachines = new CNCMonitorByMachine[]
            {
                TestCNCMonitorByMachineFactory.Create(),
            };
            IAnalyzeMachineToolUtilizationRateUseCase analyzeMachineToolUtilizationRateUseCase =
                new AnalyzeMachineToolUtilizationRateUseCase(mock_analyzer.Object);
            _ = await analyzeMachineToolUtilizationRateUseCase.ExecuteAsync(cncMonitorByMachines);

            // then
            mock_analyzer.Verify(x => x.Analyze(It.IsAny<IEnumerable<CNCMonitorByMachine>>()), Times.Once);
        }
    }
}