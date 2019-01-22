:: docker run --rm -d --name rabbitmq -p 15672:15672 -p 5672:5672 micdenny/rabbitmq-windows

start DemoRPC_Server\bin\Debug\DemoRPC_Server.exe amqp://guest:guest@localhost:5672/
start DemoRPC_Client\bin\Debug\DemoRPC_Client.exe amqp://guest:guest@localhost:5672/

:: start docker run -t -i --rm demorpc_client
:: start docker run -t -i --rm demorpc_server



