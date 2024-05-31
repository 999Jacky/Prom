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

        [AllowedValues(ServerMode, ClientMode)]
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
        public Dictionary<string, string>? Labels { get; set; }
    }
    public class ServerSetting {
        public string ClearJobCron { get; set; }
    }

    public class OnHangFireSettingChange {
        private readonly IBackgroundJobClient _jobClient;
        private readonly JobStorage _jobStorage;
        private readonly IDisposable? _onChangeToken;
        public OnHangFireSettingChange(IOptionsMonitor<HangFireConst> constMonitor, IBackgroundJobClient jobClient, JobStorage jobStorage) {
            _jobClient = jobClient;
            _jobStorage = jobStorage;
            _onChangeToken = constMonitor.OnChange(OnChange);
            this.RegisterJob(constMonitor.CurrentValue);
        }

        private void OnChange(HangFireConst option) {
            foreach (var job in _jobStorage.GetConnection().GetRecurringJobs()) {
                RecurringJob.RemoveIfExists(job.Id);
            }
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
