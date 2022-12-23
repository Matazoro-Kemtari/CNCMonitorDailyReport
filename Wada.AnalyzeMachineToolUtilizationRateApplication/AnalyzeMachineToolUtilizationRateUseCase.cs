using Wada.AOP.Logging;
using Wada.CNCMonitor.CNCMonitorAggregation;
using Wada.CNCMonitor.DomainService;
using Wada.CNCMonitor.UtilizationRateAggregation;

[module: Logging]
namespace Wada.AnalyzeMachineToolUtilizationRateApplication
{
    public interface IAnalyzeMachineToolUtilizationRateUseCase
    {
        Task<CNCUtilizationRateReport> ExecuteAsync(IEnumerable<CNCMonitorByMachine> cncMonitorByMachines);
    }
    public class AnalyzeMachineToolUtilizationRateUseCase : IAnalyzeMachineToolUtilizationRateUseCase
    {
        private readonly IMachineToolUtilizationRateAnalyzer _machineToolUtilizationRateAnalyzer;

        public AnalyzeMachineToolUtilizationRateUseCase(IMachineToolUtilizationRateAnalyzer machineToolUtilizationRateAnalyzer)
        {
            _machineToolUtilizationRateAnalyzer = machineToolUtilizationRateAnalyzer;
        }

        [Logging]
        public async Task<CNCUtilizationRateReport> ExecuteAsync(IEnumerable<CNCMonitorByMachine> cncMonitorByMachines)
        {
            return await Task.Run(() => _machineToolUtilizationRateAnalyzer.Analyze(cncMonitorByMachines));
        }
    }
}