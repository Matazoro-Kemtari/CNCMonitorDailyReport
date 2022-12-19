using Wada.CNCMonitor.CNCMonitorAggregation;
using Wada.CNCMonitor.UtilizationRateAggregation;

namespace Wada.CNCMonitor
{
    public interface IMachineToolUtilizationRateAnalyzer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cncMonitorByMachine"></param>
        /*
         * TODO: VBAのDataAggregationRunningのU列に相当の分析をする
         * グラフの色条件 DecideMachineState
        */
        CNCUtilizationRateReport Analyze(CNCMonitorByMachine cncMonitorByMachine);
    }
}
