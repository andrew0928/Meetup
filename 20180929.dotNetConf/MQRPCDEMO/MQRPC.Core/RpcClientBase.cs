using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQRPC.Core
{
    public abstract class RpcClientBase<TInputMessage, TOutputMessage> : IDisposable
        where TInputMessage : MessageBase
        where TOutputMessage : MessageBase
    {

        protected static Logger _logger = LogManager.GetCurrentClassLogger();

        protected string ExchangeName { get; set; }

        protected string ExchangeType { get; set; }

        protected string QueueName { get; set; }

        protected MessageBusTypeEnum BusType { get; set; }


        protected RpcClientBase(string exchangeName, string exchangeType) 
        {
            this.BusType = MessageBusTypeEnum.EXCHANGE;
            this.ExchangeName = exchangeName;
            this.ExchangeType = exchangeType;

            this.Init();

        }

        protected RpcClientBase(string queueName) //: base(queueName)
        {
            this.BusType = MessageBusTypeEnum.QUEUE;
            this.QueueName = queueName;

            this.Init();
        }

        protected virtual string ConnectionName
        {
            get
            {
                return this.GetType().FullName;
            }
        }



        protected IConnection connection = null;
        protected IModel channel = null;

        protected virtual void Init()
        {
            var connfac = MessageBusConfig.DefaultConnectionFactory;

            this.connection = connfac.CreateConnection(
                connfac.HostName.Split(','),
                this.ConnectionName);
            this.channel = this.connection.CreateModel();
            
            this.ReplyQueueName = this.channel.QueueDeclare().QueueName;
            this.ReplyQueueConsumer = new EventingBasicConsumer(channel);
            this.ReplyQueueConsumer.Received += this.ReplyQueue_Received;
            this.channel.BasicConsume(this.ReplyQueueName, false, this.ReplyQueueConsumer);

        }

        public virtual void Dispose()
        {
            this.ReplyQueueConsumer.Received -= this.ReplyQueue_Received;
            this.channel.QueueDelete(ReplyQueueName);


            this.channel.Dispose();
            this.connection.Dispose();
        }

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
            props.ReplyTo = ReplyQueueName;

            if (this.BusType == MessageBusTypeEnum.EXCHANGE)
            {
                channel.BasicPublish(
                    exchange: this.ExchangeName ?? "",
                    routingKey: routing,
                    basicProperties: props,
                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            }
            else if (this.BusType == MessageBusTypeEnum.QUEUE)
            {
                channel.BasicPublish(
                    exchange: "",
                    routingKey: this.QueueName,
                    basicProperties: props,
                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            }
            return correlationId;
        }


        //
        //
        //
        //
        //
        //
        //
        //
        //
        //

        protected string ReplyQueueName = null;
        protected EventingBasicConsumer ReplyQueueConsumer = null;


        private void ReplyQueue_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var props = e.BasicProperties;
            TOutputMessage output = JsonConvert.DeserializeObject<TOutputMessage>(
                Encoding.UTF8.GetString(body));

            lock (this._sync)
            {
                this.buffer[props.CorrelationId] = output;
                this.waitlist[props.CorrelationId].Set();
            }
        }

        //protected override void Init()
        //{
        //    base.Init();

        //    this.ReplyQueueName = this.channel.QueueDeclare().QueueName;
        //    this.ReplyQueueConsumer = new EventingBasicConsumer(channel);
        //    this.ReplyQueueConsumer.Received += this.ReplyQueue_Received;
        //    this.channel.BasicConsume(this.ReplyQueueName, false, this.ReplyQueueConsumer);

        //}

        private object _sync = new object();
        protected Dictionary<string, TOutputMessage> buffer = new Dictionary<string, TOutputMessage>();
        protected Dictionary<string, AutoResetEvent> waitlist = new Dictionary<string, AutoResetEvent>();

        //public override void Dispose()
        //{
        //    this.ReplyQueueConsumer.Received -= this.ReplyQueue_Received;

        //    this.channel.QueueDelete(ReplyQueueName);

        //    base.Dispose();
        //}

        //protected virtual void MessageProps_BeforePublish(IBasicProperties props)
        //{
        //    if (string.IsNullOrEmpty(ReplyQueueName) == false)
        //    {
        //        props.ReplyTo = ReplyQueueName;
        //    }
        //}

        public async Task<TOutputMessage> CallAsync(string routing, TInputMessage intput, Dictionary<string, object> headers)
        {
            string correlationId = this.PublishMessage(
                routing,
                intput,
                null);

            AutoResetEvent wait = new AutoResetEvent(false);
            lock (this._sync)
            {
                this.waitlist[correlationId] = wait;
            }

            await Task.Run(() => wait.WaitOne());

            lock (this._sync)
            {
                var output = this.buffer[correlationId];
                this.buffer.Remove(correlationId);
                this.waitlist.Remove(correlationId);

                return output;
            }
        }
    }

}
