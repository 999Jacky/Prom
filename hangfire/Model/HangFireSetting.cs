using System.ComponentModel.DataAnnotations;
using Hangfire;
using hangfire.Job;
using Hangfire.Storage;
using Microsoft.Extensions.Options;

namespace hangfire.Model {
    public class HangFireConst {
        public const string ConfigureName = "HangFire";
        public const string ServerMode = "Server";
        public const string ClientMode = "Client";

        [Required]
        public string PushGatewayUrl { get; set; }

        [Required]
        public string Mode { get; set; }

        public ClientSetting? ClientSetting { get; set; }
        public ServerSetting? ServerSetting { get; set; }
    }
    public class ClientSetting {
        public string JobName { get; set; }
        public string Instance { get; set; }

        public string PushJobCron { get; set; }
        public string ExporterUrl { get; set; }
        public string? ExecFile { get; set; }
        public string? ExecArgs { get; set; }
        public Dictionary<string, string>? Labels { get; set; }
    }
    public class ServerSetting {
        public string ClearJobCron { get; set; }
        public string? ExecFile { get; set; }
        public string? ExecArgs { get; set; }
    }

    public class OnHangFireSettingChange {
        private readonly JobStorage _jobStorage;
        private readonly IDisposable? _onChangeToken;

        // private readonly StartupService _startupService;
        public OnHangFireSettingChange(IOptionsMonitor<HangFireConst> constMonitor, JobStorage jobStorage) {
            _jobStorage = jobStorage;
            // _startupService = startupService;
            _onChangeToken = constMonitor.OnChange(OnChange);
            this.RegisterJob(constMonitor.CurrentValue);
        }

        private void OnChange(HangFireConst option) {
            foreach (var job in _jobStorage.GetConnection().GetRecurringJobs()) {
                RecurringJob.RemoveIfExists(job.Id);
            }
            // _ = _startupService.Restart();
            RegisterJob(option);
        }

        private void RegisterJob(HangFireConst option) {
            switch (option.Mode) {
                case HangFireConst.ClientMode:
                    if (option.ClientSetting == null) {
                        throw new Exception("缺少Client設定");
                    }
                    AddJob<PushMetricToServer, int>(option.ClientSetting.PushJobCron, 0);
                    break;
                case HangFireConst.ServerMode:
                    if (option.ServerSetting == null) {
                        throw new Exception("缺少Server設定");
                    }
                    AddJob<ClearTimeoutJob, int>(option.ServerSetting.ClearJobCron, 0);
                    break;
            }
        }

        private void AddJob<T, Targs>(string cron, Targs args) where T : IJob<Targs> {
            var jobName = typeof(T).Name;
            RecurringJob.AddOrUpdate<T>(jobName, j => j.Run(args), cron, new RecurringJobOptions() {
                TimeZone = TimeZoneInfo.Local
            });
        }
    }
}
