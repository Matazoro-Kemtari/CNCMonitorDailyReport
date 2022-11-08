namespace Wada.CNCMonitor
{
    public interface ICNCMonitorLoader
    {
        public Task<CNCMonitorByMachine> LoadMachineLogsAsync(StreamReader reader, PickingCNCMonitor pickingCNCMonitor);
    }
}
