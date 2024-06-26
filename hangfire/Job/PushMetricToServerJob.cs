using System.Net.Http.Headers;
using System.Text;
using Hangfire;
using hangfire.Model;
using Microsoft.Extensions.Options;

namespace hangfire.Job {
    [AutomaticRetry(Attempts = 2)]
    public class PushMetricToServer: IJob<int> {
        private IHttpClientFactory _httpClientFactory;
        private IOptionsMonitor<HangFireConst> _options;
        public PushMetricToServer(IHttpClientFactory httpClientFactory, IOptionsMonitor<HangFireConst> options) {
            _httpClientFactory = httpClientFactory;
            _options = options;
        }
        public async Task Run(int args) {
            if (_options.CurrentValue.Mode != HangFireConst.ClientMode) {
                return;
            }
            var httpClient = _httpClientFactory.CreateClient();
            var exporterResponse = await httpClient.GetStringAsync(_options.CurrentValue.ClientSetting!.ExporterUrl);
            var content = new StringContent(exporterResponse);
            var urlPamas = _options.CurrentValue.ClientSetting.JobName;
            urlPamas += $"/instance/{_options.CurrentValue.ClientSetting.Instance}";
            foreach (var label in _options.CurrentValue?.ClientSetting?.Labels ?? new Dictionary<string, string>()) {
                urlPamas += $"/{label.Key}/{label.Value}";
            }
            if (_options.CurrentValue.BasicAuth != null && !string.IsNullOrWhiteSpace(_options.CurrentValue.BasicAuth.UserName)) {
                var byteArray = Encoding.ASCII.GetBytes($"{_options.CurrentValue.BasicAuth.UserName}:{_options.CurrentValue.BasicAuth.Password}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            var resp = await httpClient.PostAsync($"{_options.CurrentValue.PushGatewayUrl}/metrics/job/{urlPamas}", content);
            var code = resp.StatusCode;
        }
    }
}
