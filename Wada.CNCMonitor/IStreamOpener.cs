namespace Wada.CNCMonitor
{
    public interface IStreamOpener
    {
        /// <summary>
        /// ストリームリーダーを開く
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        StreamReader Open(string path);
    }
}