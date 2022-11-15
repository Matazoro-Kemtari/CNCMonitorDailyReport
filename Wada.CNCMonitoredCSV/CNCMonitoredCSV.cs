using NLog;
using System.Net;
using System.Reflection;
using Wada.CNCMonitor;

namespace Wada.CNCMonitoredCSV
{
    public class CNCMonitoredCSV : ICNCMonitorLoader
    {
        private readonly ILogger logger;

        public CNCMonitoredCSV(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CNCMonitorByMachine> LoadMachineLogsAsync(StreamReader reader, PickingCNCMonitor pickingCNCMonitor)
        {
            logger.Debug("Start {0}", MethodBase.GetCurrentMethod()?.Name);

            bool readedLogedDate = false;
            DateTime loggedDate = DateTime.MinValue;
            bool readedMachineName = false;
            string machineName = string.Empty;
            IPAddress ipAddress = pickingCNCMonitor.IPAddress;
            bool readedHeader = false;
            List<CNCMonitorRecord> records = new();

            // 末尾まで繰り返す
            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();

                // 1行目 日時を読み込む
                if (!readedLogedDate)
                {
                    (loggedDate, readedLogedDate) = ReadLoggedDate(line);
                    continue;
                }
                // 2行目 設備名を読み込む
                else if (!readedMachineName)
                {
                    (machineName, ipAddress, readedMachineName) = ReadMachineName(line);
                    continue;
                }
                // 3行目 ヘッダ
                else if (!readedHeader)
                {
                    readedHeader = true;
                    continue;
                }

                CNCMonitorRecord record = ReadMonitorRecord(line);
                records.Add(record);
            }


            if (records.Count == 0)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }
            logger.Info("CNC稼働設備ログ {0}, {1}, {2} 件", loggedDate, machineName, records.Count);

            logger.Debug("Finish {0}", MethodBase.GetCurrentMethod()?.Name);
            return new CNCMonitorByMachine(
                loggedDate, pickingCNCMonitor.Factory, ipAddress, machineName, records);
        }

        private CNCMonitorRecord ReadMonitorRecord(string? line)
        {
            if (line == null)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            string[] values = line.Split(",");

            if (values.Length < 2)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            DateTime recordTime = DateTime.Parse(values[0]);
            string state = values[1];
            string programName = string.Empty;
            int feedRate = 0, spindleRotation = 0, runMode = 0, runState = 0, emergency = 0;
            if (values.Length > 2)
            {
                programName = values[2];
                feedRate = int.Parse(values[5]);
                spindleRotation = int.Parse(values[6]);
                runMode = int.Parse(values[7]);
                runState = int.Parse(values[8]);
                emergency = int.Parse(values[11]);
            }
            CNCMonitorRecord record = new(
                recordTime,
                state,
                programName,
                feedRate,
                spindleRotation,
                runMode,
                runState,
                emergency);
            return record;
        }

        private (string, IPAddress, bool) ReadMachineName(string? line)
        {
            if (line == null)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            var values = line.Split(",");

            if (values.Length < 3)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            var machineName = values[1];
            IPAddress ipAddress;
            try
            {
                ipAddress = IPAddress.Parse(values[2]);
            }
            catch (FormatException)
            {
                var msg = "CNC稼働IPアドレスが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            return (machineName, ipAddress, true);
        }

        private (DateTime, bool) ReadLoggedDate(string? line)
        {
            if (line == null)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            var values = line.Split(",");

            if (values.Length < 2)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            DateTime loggedDate;
            try
            {
                loggedDate = DateTime.Parse(values[1]);
            }
            catch (FormatException)
            {
                var msg = "CNC稼働設備ログが記録されていません";
                logger.Error(msg);
                throw new CNCMonitorLoaderException(msg);
            }

            return (loggedDate, true);
        }
    }
}