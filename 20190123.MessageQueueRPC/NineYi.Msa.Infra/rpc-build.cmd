set tags=latest
set version=0.5.7-20190128

docker build -t 91app/demo.mqrpc.consumer:%tags% -t 91app/demo.mqrpc.consumer:%version% DemoRPC_Server\bin\Debug\
docker build -t 91app/demo.mqrpc.producer:%tags% -t 91app/demo.mqrpc.producer:%version% DemoRPC_Client\bin\Debug\
::docker build -t 91app/demo.mqrpc.rabbitmq:%tags% -t 91app/demo.mqrpc.rabbitmq:%version%  rabbitmq\
docker tag andrew0928/mqrpc.rabbitmq:latest 91app/demo.mqrpc.rabbitmq:%tags%
docker tag andrew0928/mqrpc.rabbitmq:latest 91app/demo.mqrpc.rabbitmq:%version%

docker push 91app/demo.mqrpc.consumer:%tags%
docker push 91app/demo.mqrpc.consumer:%version%
docker push 91app/demo.mqrpc.producer:%tags%
docker push 91app/demo.mqrpc.producer:%version%
docker push 91app/demo.mqrpc.rabbitmq:%tags%
docker push 91app/demo.mqrpc.rabbitmq:%version%


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