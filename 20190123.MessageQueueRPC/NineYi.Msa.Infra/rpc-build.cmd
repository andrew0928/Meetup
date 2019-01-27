set tags=latest
set version=0.5.4-demo20190128

docker build -t andrew0928/mqrpc.consumer:%tags% -t andrew0928/mqrpc.consumer:%version% DemoRPC_Server\bin\Debug\
docker build -t andrew0928/mqrpc.producer:%tags% -t andrew0928/mqrpc.producer:%version% DemoRPC_Client\bin\Debug\
::docker build -t andrew0928/mqrpc.rabbitmq:%tags% -t andrew0928/mqrpc.rabbitmq:%version%  rabbitmq\

docker push andrew0928/mqrpc.consumer:%tags%
docker push andrew0928/mqrpc.consumer:%version%
docker push andrew0928/mqrpc.producer:%tags%
docker push andrew0928/mqrpc.producer:%version%
::docker push andrew0928/mqrpc.rabbitmq:%tags%
::docker push andrew0928/mqrpc.rabbitmq:%version%


goto exit

:: 啟動步驟:
:: - 先啟動 rabbitmq, 等待1分鐘，待服務完全啟動:    docker-compose up -d rabbitmq
:: - 接著啟動 consumer: docker-compose up -d --scale consumer=3 --scale producer=5 consumer producer



docker-compose up -d rabbitmq
docker ps -a
docker inspect 
docker-compose logs -t -f rabbitmq
docker-compose up -d --scale consumer=1 --scale producer=5
docker-compose logs -t -f consumer
docker-compose up -d --scale consumer=2 --scale producer=5
docker-compose up -d --scale consumer=1 --scale producer=5
docker-compose up -d --scale consumer=0 --scale producer=5


docker-compose up -d --scale producer=5 --force-recreate --no-deps producer
docker-compose up -d --scale consumer=2 --force-recreate --no-deps consumer


:exit