# prepare windows container environment

## 1. Windows 10 Pro / Enterprise, 1809

請安裝 windows 10 pro / enterprise 版本, 更新至 1809, 安裝 docker for windows 及 docker-compose

* [Install Docker Desktop for Windows](https://docs.docker.com/docker-for-windows/install/)
* [Install Docker Compose](https://docs.docker.com/compose/install/)



## 2. Windows Server 2019

因為架構的關係，Windows 10 的 windows container 必須仰賴 hyper-v, 執行啟動的效能較差。若要同時開啟多個 container 進測試，可以考慮安裝 windows server 2019 來使用。

若有這個需求，請安裝 windows server 2019 standard / data center, server core 或是 with desktop experience 都可以。請參考下列安裝程序:

* [Install Docker Engine - Enterprise on Windows Servers](https://docs.docker.com/install/windows/docker-ee/)
* [Install Docker Compose](https://docs.docker.com/compose/install/)


## 3. Cloud

如果你打算用 cloud provider 的現成 VM, 在 Azure 及 AWS 都有提供 Windows Server 2019 Data Center With Containers 的 VM 可以選用。請直接使用這個即可。除了 docker-compose 需要自行安裝之外，其他必要元件與 OS base image 都已經預先備妥了。

* [Install Docker Compose](https://docs.docker.com/compose/install/)




# Pull Images

因為 windows container 的 base image 較大，請完成安裝後預先 pull image (約需要 20 ~ 30 分鐘左右)。請是先完成下列指令:

```

docker pull andrew0928/mqrpc.producer:latest
docker pull andrew0928/mqrpc.consumer:latest
docker pull andrew0928/mqrpc.rabbitmq:latest

```