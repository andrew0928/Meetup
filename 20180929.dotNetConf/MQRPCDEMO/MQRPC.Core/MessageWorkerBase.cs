using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQRPC.Core
{
    public class MessageWorkerBase<TInputMessage> : MessageConsumerBase
        where TInputMessage : MessageBase
    {
        public MessageWorkerBase(string queueName) : base(queueName)
        {
        }

        public delegate void MessageWorkerProcess(TInputMessage message, string correlationId);

        public MessageWorkerProcess Process = null;


        protected override void ProcessMessage(object sender, BasicDeliverEventArgs e, IModel channel)
        {
            var body = e.Body;
            var props = e.BasicProperties;

            TInputMessage request = JsonConvert.DeserializeObject<TInputMessage>(Encoding.UTF8.GetString(body));

            try
            {
                this.Process(request, props.CorrelationId);
            }
            catch
            {
                // TODO: Log Exception
                //response = default(TOutputMessage); //new TOutputMessage();
            }

            channel.BasicAck(
                deliveryTag: e.DeliveryTag,
                multiple: false);
        }
    }

}
