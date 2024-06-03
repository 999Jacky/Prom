using System.Diagnostics;
using hangfire.Model;
using Microsoft.Extensions.Options;

namespace hangfire.Job {
    public class ClearTimeoutJob: IJob<int> {
        private readonly IOptionsMonitor<HangFireConst> _options;
        public ClearTimeoutJob(IOptionsMonitor<HangFireConst> options) {
            _options = options;
        }

        public async Task Run(int args) {
            if (_options.CurrentValue.Mode == HangFireConst.ServerMode) {
                if (_options.CurrentValue.ServerSetting == null || _options.CurrentValue.ServerSetting.ExecFile == null) {
                    throw new Exception("缺少執行檔路徑");
                }
                var p = new Process() {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo() {
                        FileName = GetAbsolutePath(_options.CurrentValue.ServerSetting.ExecFile),
                        Arguments = _options.CurrentValue.ServerSetting.ExecArgs ?? "",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                p.Start();
            }
        }
        private string GetAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.GetFullPath(path, AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
