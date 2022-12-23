using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.CNCMonitor.DomainService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Wada.CNCMonitor.CNCMonitorAggregation;

namespace Wada.CNCMonitor.DomainService.Tests
{
    [TestClass()]
    public class MachineToolUtilizationRateAnalyzerTests
    {
        [TestMethod()]
        public void AnalyzeTest()
        {
            // given
            Mock<IConfiguration> mock_config = new();

            // when
            IMachineToolUtilizationRateAnalyzer machineToolUtilizationRateAnalyzer =
                new MachineToolUtilizationRateAnalyzer(mock_config.Object);
            IEnumerable<CNCMonitorByMachine> cncMonitorByMachines = null;
            _ = machineToolUtilizationRateAnalyzer.Analyze(cncMonitorByMachines);

            // then
            Assert.Fail();
        }
    }
}