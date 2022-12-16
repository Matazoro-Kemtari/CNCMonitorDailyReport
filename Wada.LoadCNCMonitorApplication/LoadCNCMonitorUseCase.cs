using Microsoft.Extensions.Configuration;
using System.Net;
using Wada.AOP.Logging;
using Wada.CNCMonitor;
using Wada.CNCMonitor.CNCMonitorAggregation;

[module: Logging]
namespace Wada.LoadCNCMonitorApplication
{
    public interface ILoadCNCMonitorUseCase
    {
        /// <summary>
        /// CNCモニタ情報を読み込む
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<CNCMonitorByMachine>> ExecuteAsync(DateTime processDate);
    }

    public class LoadCNCMonitorUseCase : ILoadCNCMonitorUseCase
    {
        private readonly IConfiguration configuration;
        private readonly ICNCMonitorLoader cncMonitorLoader;
        private readonly IStreamOpener streamOpner;

        public LoadCNCMonitorUseCase(IConfiguration configuration,
                                     IStreamOpener streamOpner,
                                     ICNCMonitorLoader cncMonitorLoader)
        {
            this.configuration = configuration;
            this.streamOpner = streamOpner;
            this.cncMonitorLoader = cncMonitorLoader;
        }

        [Logging]
        public async Task<IEnumerable<CNCMonitorByMachine>> ExecuteAsync(DateTime processDate)
        {
            // モニタログ取得設定を準備する
            IEnumerable<CNCMonitorLog>? cncMonitorLogs = configuration.GetSection("cncMonitorLogs").Get<CNCMonitorLog[]>();
            if (cncMonitorLogs == null)
            {
                var m = "設定ファイルが読み込めません <cncMonitorLogs>";
                throw new LoadCNCMonitorException(m);
            }
            var pickingMonitors = cncMonitorLogs
                .Select(x => new PickingCNCMonitor(
                    processDate,
                    x.Factory,
                    IPAddress.Parse(x.IPAddress),
                    x.MachineName));

            // モニタログディレクトリ取得
            var baseDirectory = configuration.GetValue<string>("monitorLogDirectory");
            if (baseDirectory == null)
            {
                var m = "設定ファイルが読み込めません <monitorLogDirectory>";
                throw new LoadCNCMonitorException(m);
            }
            // モニタログ読み込みループ
            IEnumerable<Task<CNCMonitorByMachine>> loadTasks =
                pickingMonitors.Select(async x =>
                {
                    // ファイルを開く
                    using StreamReader stream = streamOpner.Open(x.GetFilePath(baseDirectory));

                    // データ展開
                    return await cncMonitorLoader.LoadMachineLogsAsync(
                        new LoadMachineLogsRecord(stream, x.Factory, x.IPAddress)); ;
                });

            CNCMonitorByMachine[] cncMonitorByMachines;
            try
            {
                cncMonitorByMachines = await Task.WhenAll(loadTasks);
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                var m = "CNC稼働ログ読み込みに失敗しました";
                throw new LoadCNCMonitorException(m, e);
            }

            return cncMonitorByMachines;
        }
    }
}