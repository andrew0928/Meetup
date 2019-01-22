# Message Queue (using: RabbitMQ)



Start local rabbitmq service (windows):

```
docker run --rm -d --name rabbitmq -p 15672:15672 -p 5672:5672 micdenny/rabbitmq-windows
```


Start local rabbitmq service (linux):

```
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management-alpine
```

chco






