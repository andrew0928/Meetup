using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQRPC.Core
{
    public class RpcServerBase<TInputMessage, TOutputMessage> : MessageConsumerBase
        where TInputMessage : MessageBase
        where TOutputMessage : MessageBase
    {

        public RpcServerBase(string queueName) : base(queueName)
        {
        }


        public delegate TOutputMessage MessageWorkerProcess(TInputMessage message, string correlationId);

        public MessageWorkerProcess Process = null;

        protected override void ProcessMessage(object sender, BasicDeliverEventArgs e, IModel channel)
        {
            var body = e.Body;
            var props = e.BasicProperties;
            TOutputMessage response = null;
            TInputMessage request = JsonConvert.DeserializeObject<TInputMessage>(Encoding.UTF8.GetString(body));

            try
            {
                response = this.Process(request, props.CorrelationId);
            }
            catch
            {
                // TODO: Log Exception
                //response = default(TOutputMessage); //new TOutputMessage();
            }

            channel.BasicAck(
                deliveryTag: e.DeliveryTag,
                multiple: false);

            // send response


            if (string.IsNullOrEmpty(props.ReplyTo) == false)
            {
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                byte[] responseBytes = null;//Encoding.UTF8.GetBytes(response);
                if (response != null)
                {
                    responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                }

                channel.BasicPublish(
                    exchange: "",
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: responseBytes);
            }
        }
    }

}
