using System.Diagnostics;
using hangfire.Model;
using Microsoft.Extensions.Options;

namespace hangfire {
    public class StartupService {
        public static readonly List<Process> Processes = new List<Process>();
        private readonly IOptionsMonitor<HangFireConst> _options;
        public StartupService(IOptionsMonitor<HangFireConst> options) {
            _options = options;
        }
        public void Startup() {
            if (_options.CurrentValue.Mode == HangFireConst.ClientMode) {
                if (_options.CurrentValue.ClientSetting == null || _options.CurrentValue.ClientSetting.ExecFile == null) {
                    throw new Exception("缺少執行檔路徑");
                }
                var p = new Process() {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo() {
                        FileName = GetAbsolutePath(_options.CurrentValue.ClientSetting.ExecFile),
                        Arguments = _options.CurrentValue.ClientSetting.ExecArgs ?? "",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                Processes.Add(p);
            }
        }

        public async Task Shutdown() {
            foreach (var process in Processes) {
                if (!process.HasExited) {
                    process.Kill();
                    await process.WaitForExitAsync();
                }
            }
            Processes.Clear();
        }

        public async Task Restart() {
            await Shutdown();
            Startup();
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
