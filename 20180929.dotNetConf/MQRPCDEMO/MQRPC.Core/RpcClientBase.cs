using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQRPC.Core
{
    public abstract class RpcClientBase<TInputMessage, TOutputMessage> : MessageProducerBase<TInputMessage>
        where TInputMessage : MessageBase
        where TOutputMessage : MessageBase
    {

        protected RpcClientBase(string exchangeName, string exchangeType) : base(exchangeName, exchangeType)
        {
        }

        protected RpcClientBase(string queueName) : base(queueName)
        {
        }

        protected string ReplyQueueName = null;
        protected EventingBasicConsumer ReplyQueueConsumer = null;


        private void ReplyQueue_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var props = e.BasicProperties;
            TOutputMessage output = JsonConvert.DeserializeObject<TOutputMessage>(Encoding.UTF8.GetString(body));

            lock (this._sync)
            {
                this.buffer[props.CorrelationId] = output;
                this.waitlist[props.CorrelationId].Set();
            }
        }

        protected override void Init()
        {
            base.Init();

            this.ReplyQueueName = this.channel.QueueDeclare().QueueName;
            this.ReplyQueueConsumer = new EventingBasicConsumer(channel);
            this.ReplyQueueConsumer.Received += this.ReplyQueue_Received;
            this.channel.BasicConsume(this.ReplyQueueName, false, this.ReplyQueueConsumer);

        }

        private object _sync = new object();
        protected Dictionary<string, TOutputMessage> buffer = new Dictionary<string, TOutputMessage>();
        protected Dictionary<string, AutoResetEvent> waitlist = new Dictionary<string, AutoResetEvent>();

        public override void Dispose()
        {
            this.ReplyQueueConsumer.Received -= this.ReplyQueue_Received;

            this.channel.QueueDelete(ReplyQueueName);

            base.Dispose();
        }

        protected override void MessageProps_BeforePublish(IBasicProperties props)
        {
            base.MessageProps_BeforePublish(props);

            if (string.IsNullOrEmpty(ReplyQueueName) == false)
            {
                props.ReplyTo = ReplyQueueName;
            }
        }

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
