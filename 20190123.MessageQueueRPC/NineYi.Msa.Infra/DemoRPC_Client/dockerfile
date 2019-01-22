FROM mcr.microsoft.com/windows/servercore:ltsc2019

WORKDIR c:/demorpc_client
COPY . .

ENV MQURL=amqp://guest:guest@rabbitmq:5672/

CMD demorpc_client.exe %MQURL%