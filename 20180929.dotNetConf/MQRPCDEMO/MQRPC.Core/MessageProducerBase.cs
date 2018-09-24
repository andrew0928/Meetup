//#define USE_LOGTRACKER
using MQRPC.Core;
using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MQRPC.Core
{
    public abstract class MessageProducerBase<TInputMessage> : IDisposable
        where TInputMessage : MessageBase
    {



        //protected MessageProducerBase(string exchangeName, string exchangeType)
        //{
        //    this.BusType = MessageBusTypeEnum.EXCHANGE;
        //    this.ExchangeName = exchangeName;
        //    this.ExchangeType = exchangeType;

        //    this.Init();
        //}

        //protected MessageProducerBase(string queueName)
        //{
        //    this.BusType = MessageBusTypeEnum.QUEUE;
        //    this.QueueName = queueName;

        //    this.Init();
        //}


        //protected virtual string ConnectionName
        //{
        //    get
        //    {
        //        return this.GetType().FullName;
        //    }
        //}

        //protected IConnection connection = null;
        //protected IModel channel = null;

        //protected virtual void Init()
        //{
        //    var connfac = MessageBusConfig.DefaultConnectionFactory;

        //    this.connection = connfac.CreateConnection(
        //        connfac.HostName.Split(','), 
        //        this.ConnectionName);
        //    this.channel = this.connection.CreateModel();
        //}

        //public virtual void Dispose()
        //{
        //    this.channel.Dispose();
        //    this.connection.Dispose();
        //}


        //private int retryCount = 3;
        //private TimeSpan retryWaitTimeout = TimeSpan.Zero;


        private TimeSpan? messageExpirationTimeout = null;

        internal protected virtual string PublishMessage(
            string routing,
            TInputMessage message,
            Dictionary<string, object> messageHeaders)
        {
            string correlationId = Guid.NewGuid().ToString("N");

            if (this.BusType == MessageBusTypeEnum.QUEUE)
            {
                channel.QueueDeclare(
                    //queue: routing,
                    queue: this.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
            else if (this.BusType == MessageBusTypeEnum.EXCHANGE)
            {
                channel.ExchangeDeclare(
                    this.ExchangeName,
                    this.ExchangeType,
                    true,
                    false,
                    null);
            }
            else
            {
                throw new InvalidOperationException();
            }
                        



            IBasicProperties props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            if (messageExpirationTimeout != null)
            {
                props.Expiration = messageExpirationTimeout.Value.TotalMilliseconds.ToString();
            }

            props.Headers = (messageHeaders == null) ?
                new Dictionary<string, object>() :
                new Dictionary<string, object>(messageHeaders);

            props.CorrelationId = correlationId; //Guid.NewGuid().ToString("N");

            this.MessageProps_BeforePublish(props);
                        

            if (this.BusType == MessageBusTypeEnum.EXCHANGE)
            {
                channel.BasicPublish(
                    exchange: this.ExchangeName ?? "",
                    routingKey: routing,
                    basicProperties: props,
                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
                    //body: messageBody);
                                    
            }
            else if (this.BusType == MessageBusTypeEnum.QUEUE)
            {
                channel.BasicPublish(
                    exchange: "",
                    routingKey: this.QueueName,
                    basicProperties: props,
                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
                    //body: messageBody);
            }
            return correlationId;
        }

        protected virtual void MessageProps_BeforePublish(IBasicProperties props)
        {

        }

    }



}
