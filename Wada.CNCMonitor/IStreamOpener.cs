namespace Wada.CNCMonitor
{
    public interface IStreamOpener
    {
        StreamReader Open(string path);
    }
}