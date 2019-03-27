# Labs


## DEMO 1, basic docker usage


啟動一個 windows server core container, 進入命令列模式:
```
docker run -t -i mcr.microsoft.com/windows/servercore:1809 cmd.exe
```

啟動一個 IIS container:
```
docker run -t -i -p 80:80 mcr.microsoft.com/windows/servercore/iis:windowsservercore-ltsc2019
```

啟動一個 IIS container, 並且將 %CD%\website\ 掛載到 container 內的 c:\inetpub\wwwroot\ :
```
docker run -t -i --rm --name demoiis -p 80:80 -v %CD%\website\:c:\inetpub\wwwroot\ mcr.microsoft.com/windows/servercore/iis:windowsservercore-ltsc2019
```

用 docker exec 指令，在同一個 container 內，開另一個終端機，連接到 cmd.exe 命令提示字元:

```
docker exec -t -i demoiis cmd.exe
```



## DEMO 2 (VS20)

啟動一個 ASP.NET container, 並且將 %CD%\webapp\ (asp.net **webform** webapplication) 掛載到 container 內的 c:\inetpub\wwwroot\ :

```
docker run -d --rm -p 80:80 -v %CD%\webapp\:c:\inetpub\wwwroot\ mcr.microsoft.com/dotnet/framework/aspnet:4.7.2-windowsservercore-ltsc2019
```

較好的做法, container 不應該跟 host 環境 (本機路徑 %CD%\webapp) 有相依性, 因此應該想辦法把 code
也包到 container image 內。正規的做法是撰寫 ```dockerfile```:

```
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.7.2-windowsservercore-ltsc2019

ADD . c:/inetpub/wwwroot
```

然後 build:

```
docker build -t demoasp .
```

最後直接啟動指定的 image:

```
docker run -d --rm -p 80:80 demoasp
```


## DEMO 3 (VS20, container security)

在 windows 2019 啟動一個 windows servercore container, 並且執行 ping -t 8.8.8.8

```
docker run -t -i --rm mcr.microsoft.com/windows/servercore:1809 ping.exe -t 8.8.8.8
```

在 host 另外開啟 cmd.exe, 執行 tasklist 找看看有無 ping.exe ?

```
tasklist | find /i "ping"
```

開啟工作管理員, 切到 detail, 開啟 command line 欄位, 找到 ping.exe, 你會發現你看的到 ping.exe -t 8.8.8.8, 看的到命令列執行的參數。

> 如果你的參數包含機密資訊呢? 你是否還敢在你不信任的 host 執行你的 container? 尤其是其他平台業者提供的 **免費** 環境，讓你免費啟動你的 container ...

更進一步，用工作管理員 create dump, find /i "ping.exe" mydump 看看:



在 windows 10 pro 重新測試一次...

在 windows 2019 加上 --isolation=hyperv