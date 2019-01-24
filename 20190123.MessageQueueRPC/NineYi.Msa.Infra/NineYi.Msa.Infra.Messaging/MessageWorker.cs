using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NineYi.Msa.Infra.Messaging.MessageKeeper;
//using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NineYi.Msa.Infra.Messaging
{

    public abstract class MessageWorkerBase : BackgroundService, IDisposable
    {
        protected MessageWorkerOptions _options { get; set; } = MessageWorkerOptions.Default;


        protected ILogger _logger { get; set; } = null;

        protected IServiceProvider _services { get; set; } = null;

        protected readonly IMessageKeeper _messageKeeper;


        // TO\FROM      | STOPPED(0) | STARTING(1)  | STARTED(2)    | STOPPING(3)
        // ----------------------------------------------------------------------
        // STOPPED(0)   | true       | false        | false         | true
        // STARTING(1)  | true       | true         | false         | false
        // STARTED(2)   | false      | true         | true          | false
        // STOPPING(3)  | false      | false        | true          | true

        protected IConnection _connection = null;

        public MessageWorkerBase(
            MessageWorkerOptions options,
            ILogger<MessageWorkerBase> logger,
            //IMessageKeeper messageKeeper,
            IServiceProvider services)
        {
            this._options = options;
            this._logger = logger;
            //this._messageKeeper = messageKeeper;
            this._services = services;

            if (this._logger == null) this._logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<MessageWorkerBase>();
        }

        private object _sync_root = new object();

        protected int _subscriber_received_count = 0;
        protected AutoResetEvent _subscriber_received_wait = new AutoResetEvent(false);

        protected abstract void Subscriber_Received(object sender, BasicDeliverEventArgs e, IModel channel);


        public override void Dispose()
        {
            if (this._connection != null)
            {
                this._connection.Close();
            }
            base.Dispose();
        }

        protected async virtual Task InitConnectionAsync()
        {
            if (string.IsNullOrWhiteSpace(this._options.QueueName)) throw new ArgumentNullException("QueueName");

            var cf = new ConnectionFactory()
            {
                Uri = new Uri(this._options.ConnectionURL)
            };

            for (int retry_count = this._options.ConnectionRetryCount; retry_count > 0; retry_count--)
            {
                try
                {
                    this._connection = cf.CreateConnection(this._options.ConnectionName);
                    break;
                }
                catch
                {
                    if (this._connection != null) this._connection.Close();
                }

                this._logger.LogWarning("connection create fail. restarting...");
                retry_count--;
                if (retry_count == 0) throw new Exception("Retry 次數已超過，不再重新嘗試連線。");

                await Task.Delay(this._options.ConnectionRetryTimeout);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.InitConnectionAsync();

            // init multiple (worker_count) channel(s)...
            List<IModel> channels = new List<IModel>();
            List<EventingBasicConsumer> consumers = new List<EventingBasicConsumer>();
            List<EventHandler<BasicDeliverEventArgs>> handlers = new List<EventHandler<BasicDeliverEventArgs>>();

            for (int index = 0; index < this._options.WorkerThreadsCount; index++)
            {
                var channel = this._connection.CreateModel();

                if (index == 0)
                {
                    // 只 declare 一次
                    channel.QueueDeclare(
                        queue: this._options.QueueName,
                        durable: this._options.QueueDurable,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                }

                var consumer = new EventingBasicConsumer(channel);
                void process_message(object sender, BasicDeliverEventArgs e) => Subscriber_Received(sender, e, channel);

                consumer.Received += process_message;
                channel.BasicQos(0, this._options.PrefetchCount, true);
                channel.BasicConsume(this._options.QueueName, false, consumer);

                channels.Add(channel);
                consumers.Add(consumer);
                handlers.Add(process_message);
            }

            await Task.Run(() => { stoppingToken.WaitHandle.WaitOne(); });

            // dispose multiple channel(s)...
            for (int index = 0; index < this._options.WorkerThreadsCount; index++)
            {

                consumers[index].Received -= handlers[index];
                handlers[index] = null;
            }
            handlers = null;

            //WaitUntilShutdown();
            while (this._subscriber_received_count > 0)
            {
                this._subscriber_received_wait.WaitOne();
            }

            for (int index = 0; index < this._options.WorkerThreadsCount; index++)
            {
                channels[index].Close();
            }

            this._connection.Close();
        }

        protected Dictionary<string, string> GetHeaders(IBasicProperties props)
        {
            if (props == null || props.IsHeadersPresent() == false) return null;


            var dict = new Dictionary<string, string>();

            foreach (var key in props.Headers.Keys)
            {
                dict.Add(key, Encoding.UTF8.GetString((byte[])props.Headers[key]));
            }

            return dict;
        }
    }

    public class MessageWorker<TInputMessage> : MessageWorkerBase where TInputMessage : MessageBase
    {

        public MessageWorker(
            MessageWorkerOptions options,
            ILogger<MessageWorker<TInputMessage>> logger,
            //IMessageKeeper messageKeeper,
            IServiceProvider services) : base(options, logger, services)
        {
        }

        public delegate void MessageWorkerProcess(TInputMessage message, string correlationId, IServiceScope scope = null);
        public MessageWorkerProcess Process = null;

        protected override void Subscriber_Received(object sender, BasicDeliverEventArgs bdeArgs, IModel channel)
        {
            if (this.Process == null)
            {
                this._logger.LogWarning($"MessageWorker<{typeof(TInputMessage).FullName}> do not define Process() delegate. Bypass process message.");
                return;
            }

            if (bdeArgs == null)
            {
                this._logger.LogWarning($"Bypass message because BasicDeliverEventArgs is null.");
                return;
            }

            Interlocked.Increment(ref this._subscriber_received_count);

            var props = bdeArgs.BasicProperties;
            using (this._logger.BeginScope(new Dictionary<string, object>
            {
                [nameof(props.CorrelationId)] = props.CorrelationId
            }))
            {
                var processOk = false;
                var message = string.Empty;
                TInputMessage input = null;

                try
                {
                    message = Encoding.UTF8.GetString(bdeArgs.Body);

                    try
                    {
                        input = JsonConvert.DeserializeObject<TInputMessage>(message);
                        this._logger.LogInformation("ProcessMsg {@Input}", input);
                    }
                    catch
                    {
                        throw new Exception($"Deserialize to {typeof(TInputMessage).FullName} failed: " + message);
                    }

                    if (this._services == null)
                    {
                        this.Process(input, props.CorrelationId, null);
                    }
                    else
                    {
                        using (var scope = this._services.CreateScope())
                        {
                            TrackContext.TryToContext(scope.ServiceProvider, this.GetHeaders(props));
                            this.Process(input, props.CorrelationId, scope);
                        }
                    }

                    processOk = true;
                    this._logger.LogInformation("ProcessMsgOk");
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "ProcessMsgError");
                }

                try
                {
                    channel.BasicAck(
                        deliveryTag: bdeArgs.DeliveryTag,
                        multiple: false);
                    this._logger.LogInformation("AckMsgOk");

                    if (!processOk)
                    {
                        if (_messageKeeper != null)
                        {
                            _messageKeeper.Save(new MessageState
                            {
                                Meta = new MessageMeta
                                {
                                    RoutingKey = bdeArgs.RoutingKey,
                                    CorrelationId = props.CorrelationId
                                },
                                Message = input ?? (object)message
                            });
                        }
                        this._logger.LogInformation("MsgDead");
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "AckMsgError");
                }
            }
            Interlocked.Decrement(ref this._subscriber_received_count);
            this._subscriber_received_wait.Set();
        }
    }

    public class MessageWorker<TInputMessage, TOutputMessage> : MessageWorkerBase
        where TInputMessage : MessageBase
        where TOutputMessage : MessageBase
    {

        public MessageWorker(
            MessageWorkerOptions options,
            ILogger<MessageWorker<TInputMessage, TOutputMessage>> logger,
            IServiceProvider services) : base(options, logger, services)
        {
            //if (this._services != null)
            //{
            //    this.Process = this._services.GetService<MessageWorkerProcess>();
            //}
        }

        public delegate TOutputMessage MessageWorkerProcess(TInputMessage message, string correlationId, IServiceScope scope = null);
        public MessageWorkerProcess Process = null;


        protected override void Subscriber_Received(object sender, BasicDeliverEventArgs e, IModel channel)
        {
            if (this.Process == null)
            {
                this._logger.LogWarning($"MessageWorker<{typeof(TInputMessage).FullName}, {typeof(TOutputMessage).FullName}> do not define Process() delegate. Bypass process message.");
                return;
            }

            if (e == null)
            {
                this._logger.LogWarning($"Bypass message because BasicDeliverEventArgs is null.");
                return;
            }

            Interlocked.Increment(ref this._subscriber_received_count);

            var props = e.BasicProperties;
            using (this._logger.BeginScope(new Dictionary<string, object>
            {
                [nameof(props.CorrelationId)] = props.CorrelationId
            }))
            {
                var message = string.Empty;
                TOutputMessage response = null;

                try
                {
                    message = Encoding.UTF8.GetString(e.Body);

                    TInputMessage input = null;
                    try
                    {
                        input = JsonConvert.DeserializeObject<TInputMessage>(message);
                        this._logger.LogInformation("ProcessMsg {@Input}", input);
                    }
                    catch
                    {
                        throw new Exception($"Deserialize to {typeof(TInputMessage).FullName} failed: " + message);
                    }

                    if (this._services == null)
                    {
                        response = this.Process(input, props.CorrelationId, null);
                    }
                    else
                    {
                        using (var scope = this._services.CreateScope())
                        {
                            TrackContext.TryToContext(scope.ServiceProvider, this.GetHeaders(props));
                            response = this.Process(input, props.CorrelationId, scope);
                        }
                    }

                    this._logger.LogInformation("ProcessMsgOk");
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "ProcessMsgError");
                }

                try
                {
                    channel.BasicAck(
                        deliveryTag: e.DeliveryTag,
                        multiple: false);

                    this._logger.LogInformation("AckMsgOk");
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "AckMsgError");
                }

                if (string.IsNullOrWhiteSpace(props.ReplyTo) == false)
                {
                    try
                    {
                        this._logger.LogInformation("ReplyMsg {@ReplyTo}", props.ReplyTo);
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;

                        byte[] responseBytes = null;
                        if (response != null)
                        {
                            responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                        }

                        channel.BasicPublish(
                            exchange: "",
                            routingKey: props.ReplyTo,
                            basicProperties: replyProps,
                            body: responseBytes);

                        this._logger.LogInformation("ReplyMsgOK");
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "ReplyMsgError");
                    }
                }
            }

            Interlocked.Decrement(ref this._subscriber_received_count);
            this._subscriber_received_wait.Set();
        }


    }
}
