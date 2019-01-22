FROM mcr.microsoft.com/windows/servercore:ltsc2019

WORKDIR c:/demorpc_server
COPY . .

ENV MQURL=amqp://guest:guest@rabbitmq:5672/

CMD demorpc_server.exe %MQURL%