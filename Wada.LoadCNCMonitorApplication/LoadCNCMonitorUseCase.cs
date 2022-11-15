using Microsoft.Extensions.Configuration;
using NLog;
using System.Net;
using System.Reflection;
using Wada.CNCMonitor;

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
        private readonly ILogger logger;
        private readonly ICNCMonitorLoader cncMonitorLoader;
        private readonly IStreamOpener streamOpner;

        public LoadCNCMonitorUseCase(IConfiguration configuration,
                                     ILogger logger,
                                     IStreamOpener streamOpner,
                                     ICNCMonitorLoader cncMonitorLoader)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.streamOpner = streamOpner;
            this.cncMonitorLoader = cncMonitorLoader;
        }

        public async Task<IEnumerable<CNCMonitorByMachine>> ExecuteAsync(DateTime processDate)
        {
            logger.Debug("Start {0}", MethodBase.GetCurrentMethod()?.Name);

            // モニタログ取得設定を準備する
            IEnumerable<CNCMonitorLog>? cncMonitorLogs = configuration.GetSection("cncMonitorLogs").Get<CNCMonitorLog[]>();
            if (cncMonitorLogs == null)
            {
                var m = "設定ファイルが読み込めません <cncMonitorLogs>";
                logger.Error(m);
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
                logger.Error(m);
                throw new LoadCNCMonitorException(m);
            }
            // モニタログ読み込みループ
            IEnumerable<Task<CNCMonitorByMachine>> loadTasks =
                pickingMonitors.Select(async x =>
                {
                    // ファイルを開く
                    using StreamReader stream = streamOpner.Open(x.GetFilePath(baseDirectory));

                    // データ展開
                    return await cncMonitorLoader.LoadMachineLogsAsync(stream, x);
                });

            CNCMonitorByMachine[] cncMonitorByMachines;
            try
            {
                cncMonitorByMachines = await Task.WhenAll(loadTasks);
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                var m = "CNC稼働ログ読み込みに失敗しました";
                logger.Error(e, m);
                throw new LoadCNCMonitorException(m, e);
            }

            logger.Debug("Finish {0}", MethodBase.GetCurrentMethod()?.Name);
            return cncMonitorByMachines;
        }
    }
}