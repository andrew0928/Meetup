


# Prepare Environment

在本機啟動 Rabbit MQ (for windows)

```
docker run --rm -d --name rabbitmq -p 15672:15672 -p 5672:5672 micdenny/rabbitmq-windows
```

啟動 N 個 Client 的指令
```
for /L %i in (1,1,10) do start /min MQRPC.Client\bin\debug\MQRPC.Client.exe
```


啟動 M 個 Server 的指令
```
for /L %i in (1,1,10) do start /min MQRPC.Server\bin\debug\MQRPC.Server.exe
```