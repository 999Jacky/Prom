
#### 密碼
* **一定要設定避免直接公開到外網上**
* 找線上bcrypt產生器,輸入密碼後貼到 Auth.yaml
``` text
basic_auth_users:
  userName1: 產生hash
  userName2: 產生hash2
```
* 這裡的密碼是要填到 prometheus的yaml設定檔裡,hash貼進windows_exporter設定檔裡



#### 服務安裝

1. 下載exe執行檔 [github](https://github.com/prometheus-community/windows_exporter)
   * 推薦用[nssm](https://nssm.cc/download)處理以下設定,有gui可以使用,要使用pre-release版
   ```Shell
   nssm install windows_exporter 
   ```
2. 建立windows服務(要使用管理員權限),記得修改參數路徑
```shell
sc create windows_exporter binpath= "E:\windows_exporter\windows_exporter-0.26.0-tags-v0-26-0-rc-2.1-amd64.exe --web.config.file=E:\windows_exporter\windowsExporterAuth.yaml --config.file=E:\windows_exporter\windowsExporterSetting.yaml" displayname= "windows_exporter" start= auto
```
3. 去windows服務啟動服務
4. http://localhost:9182 確認服務啟動**並且要輸入密碼**

+ 移除服務(要使用管理員權限)
```shell
sc delete windows_exporter
```


#### msi安裝(**未成功**)
1. 下載msi安裝包
2. 透過指令安裝(如果用點擊msi安裝的,要透過修改註冊表去修改啟動參數)
* 如果使用powershell要先設定
``` shell
$PSNativeCommandArgumentPassing = 'Legacy'
```
``` shell
msiexec /i <path-to-msi-file> EXTRA_FLAGS="--web.config.file=E:\windows_exporter\windowsExporterAuth.yaml --config.file=E:\windows_exporter\windowsExporterSetting.yaml"
```
+ 如要設定設定ip白名單
``` shell
msiexec /i <path-to-msi-file> REMOTE_ADDR="127.0.0.1,127.0.0.2" 後面同上EXTRA_FLAGS
```
