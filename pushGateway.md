# PushGateway

### 使用場景

Prometheus 無法直接連線到目標抓取指標,
統一讓目標主動Push指標到gateway上,
Prometheus再從gateway上抓取所有指標.

所有的指標**不會**逾時,需要手動刪除(官方社群表示現在,未來不會增加此功能),避免裝置離線了,Prometheus一直抓到舊的值

官方推薦使用 [PushProx](https://github.com/prometheus-community/PushProx)搭配反向代理，這個工具會轉送所有的http request
但是會讓所有連上的裝置組成**內網**,我們的使用場景會有使用者,會有資安疑慮故不採用

### server端安裝

* 下載執行檔 [PushGateway](https://github.com/prometheus/pushgateway)
* 透過nssm 安裝windows服務
* [設定手動刪除過時指標](#刪除pushgateway過時指標)

### 目標裝置安裝

* Label設定接在/job/{jobLabel}/labelName/labelValue後面(需要encode)
* 透過windows排程設定呼叫cmd,但是會彈出cmd

```cmd push.bat
@echo off
set METRICS_URL=http://localhost:9182/metrics
set PUSHGATEWAY_URL=http://localhost:9091/metrics/job/12345/instance/PC

curl %METRICS_URL% | curl --data-binary @- %PUSHGATEWAY_URL%
```

* 透過程式(hangfire)
* 參考 [appsettings.Client.json](./hangfire/appsettings.Client.json)設定
* 預設從已安裝exporter轉送指標

### 刪除PushGateway過時指標

因為.net沒有parse指標格式的lib,所以透過golang而外寫,參考[main.go](./ClearTimeout/main.go),
在執行檔傳入以下參數

+ -url pushGateway url
+ -timeout 逾時時間

可以透過windows排程或是透過[Hangfire](#hangfire專案自動啟動執行檔)觸發

### Hangfire專案自動啟動執行檔

* 在 Program.cs DI

```csharp
// 取消以下builder註解
builder.Services.AddSingleton<StartupService>();

// 取消以下Service註解
var startUpService = app.Services.GetService<StartupService>();
// 啟動執行檔
startUpService!.Startup();
// .net關閉時,連帶停止執行檔
app.Lifetime.ApplicationStopping.Register(() => startUpService.Shutdown().GetAwaiter().GetResult());
```

* 參考 [appsetting.Client.json](./hangfire/appsettings.Client.json),
[appsetting.Server.json](./hangfire/appsettings.Server.json)
中ExecFile,ExecArgs
* 在IIS中對應的應用程式集區-進階設定-識別選擇一個更高權限的帳號

### TODO
根據Prometheus設計,最好還是讓prom主動去抓各裝置的指標
寫一個新工具處理以下
+ 長時間連線(websocket,SignalR?)
+ 分辨server端(prom),和unique hostname client(手動設定hostname)
+ client端收到請求時,只從固定的http url(exporter) response回去
+ service discovery,自動從client list建立抓取目標清單