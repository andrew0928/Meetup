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
    public enum WorkerStatusEnum : int
    {
        STOPPED = 0,
        STARTING = 1,
        STARTED = 2,
        STOPPING = 3
    }

    public class RpcServerBase<TInputMessage, TOutputMessage> : IDisposable //: MessageConsumerBase
        where TInputMessage : MessageBase
        where TOutputMessage : MessageBase
    {
        protected static Logger _logger = LogManager.GetCurrentClassLogger();


        // TO\FROM      | STOPPED(0) | STARTING(1)  | STARTED(2)    | STOPPING(3)
        // ----------------------------------------------------------------------
        // STOPPED(0)   | false      | false        | false         | true
        // STARTING(1)  | true       | false        | false         | false
        // STARTED(2)   | false      | true         | false         | false
        // STOPPING(3)  | false      | false        | true          | false
        private static readonly bool[][] _state_transition_table = new bool[][]
        {
            new bool[] { true,  false, false, true },
            new bool[] { true, true,  false, false },
            new bool[] { false, true, true, false },
            new bool[] { false, false, true,  true },
        };

        public WorkerStatusEnum Status
        {
            get
            {
                return this._status;
            }
            set
            {
                lock (this._sync_root)
                {
                    if (_state_transition_table[(int)value][(int)this._status] == false) throw new InvalidOperationException($"invalid operation: {this.QueueName} from {this._status} to {value}");
                    if (this._status != value) _logger.Info($"worker status was changed: {value}");
                    this._status = value;
                }
            }
        }
        private WorkerStatusEnum _status = WorkerStatusEnum.STOPPED;

        internal string QueueName { get; set; }

        internal IConnection _connection = null;







        public RpcServerBase(string queueName) //: base(queueName)
        {
            this.QueueName = queueName;
            this.Init();
        }


        private int retry_count;
        private TimeSpan retry_timeout;

        protected virtual void Init()
        {
            this.retry_count = MessageBusConfig.DefaultRetryCount;
            this.retry_timeout = MessageBusConfig.DefaultRetryWaitTime;

            ConnectionFactory cf = MessageBusConfig.DefaultConnectionFactory;
            while (retry_count > 0)
            {
                try
                {
                    this._connection = MessageBusConfig.DefaultConnectionFactory.CreateConnection(cf.HostName.Split(','), this.ConnectionName);
                    break;
                }
                catch
                {
                    if (this._connection != null) this._connection.Close();
                }

                _logger.Warn("connection create fail. restarting...");
                retry_count--;
                if (retry_count == 0) throw new Exception("Retry 次數已超過，不再重新嘗試連線。");

                Task.Delay(retry_timeout).Wait();
            }
        }
        public virtual void Dispose()
        {
            this.StopWorkersAsync().Wait();

            if (this._connection != null)
            {
                this._connection.Close();
            }
        }


        protected virtual string ConnectionName
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        private ManualResetEvent _stop_wait = new ManualResetEvent(true);

        private object _sync_root = new object();


        public async Task StartWorkersAsync(int worker_count = 1)
        {
            this.Status = WorkerStatusEnum.STARTING;
            this._stop_wait.Reset();
            this.Status = WorkerStatusEnum.STARTED;

            // init multiple (worker_count) channel(s)...
            List<IModel> channels = new List<IModel>();
            List<EventingBasicConsumer> consumers = new List<EventingBasicConsumer>();
            List<EventHandler<BasicDeliverEventArgs>> handlers = new List<EventHandler<BasicDeliverEventArgs>>();
            for (int index = 0; index < worker_count; index++)
            {
                var channel = this._connection.CreateModel();

                if (index == 0)
                {
                    // 只 declare 一次
                    channel.QueueDeclare(
                        queue: this.QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                }

                EventHandler<BasicDeliverEventArgs> x = (sender, e) => Subscriber_Received(sender, e, channel);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += x;
                channel.BasicConsume(this.QueueName, false, consumer);

                channels.Add(channel);
                consumers.Add(consumer);
                handlers.Add(x);
            }

            await Task.Run(() => { this._stop_wait.WaitOne(); });

            // dispose multiple channel(s)...
            for (int index = 0; index < worker_count; index++)
            {
                consumers[index].Received -= handlers[index];
                channels[index].Close();

                channels[index] = null;
                consumers[index] = null;
                handlers[index] = null;
            }
            channels.Clear(); channels = null;
            consumers.Clear(); consumers = null;
            handlers.Clear(); handlers = null;

            await this.WaitUntilShutdownAsync();

            this.Status = WorkerStatusEnum.STOPPED;
        }



        
        public async Task StopWorkersAsync()
        {
            if (this.Status == WorkerStatusEnum.STOPPED) return;

            this.Status = WorkerStatusEnum.STOPPING;
            this._stop_wait.Set();

            await this.WaitUntilShutdownAsync();
        }
        private async Task WaitUntilShutdownAsync()
        {
            await Task.Run(() => {
                while (this._subscriber_received_count > 0)
                {
                    this._subscriber_received_wait.WaitOne(1000);
                }
            });
        }

        private int _subscriber_received_count = 0;
        private AutoResetEvent _subscriber_received_wait = new AutoResetEvent(false);


        //protected abstract void ProcessMessage(object sender, BasicDeliverEventArgs e, IModel channel);

        public delegate TOutputMessage MessageWorkerProcess(TInputMessage message, string correlationId);

        public MessageWorkerProcess Process = null;

        protected void Subscriber_Received(object sender, BasicDeliverEventArgs e, IModel channel)
        {
            Interlocked.Increment(ref this._subscriber_received_count);
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
            Interlocked.Decrement(ref this._subscriber_received_count);
            this._subscriber_received_wait.Set();
        }

























        //protected void ProcessMessage(object sender, BasicDeliverEventArgs e, IModel channel)
        //{
        //    var body = e.Body;
        //    var props = e.BasicProperties;
        //    TOutputMessage response = null;
        //    TInputMessage request = JsonConvert.DeserializeObject<TInputMessage>(Encoding.UTF8.GetString(body));

        //    try
        //    {
        //        response = this.Process(request, props.CorrelationId);
        //    }
        //    catch
        //    {
        //        // TODO: Log Exception
        //        //response = default(TOutputMessage); //new TOutputMessage();
        //    }

        //    channel.BasicAck(
        //        deliveryTag: e.DeliveryTag,
        //        multiple: false);

        //    // send response


        //    if (string.IsNullOrEmpty(props.ReplyTo) == false)
        //    {
        //        var replyProps = channel.CreateBasicProperties();
        //        replyProps.CorrelationId = props.CorrelationId;

        //        byte[] responseBytes = null;//Encoding.UTF8.GetBytes(response);
        //        if (response != null)
        //        {
        //            responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
        //        }

        //        channel.BasicPublish(
        //            exchange: "",
        //            routingKey: props.ReplyTo,
        //            basicProperties: replyProps,
        //            body: responseBytes);
        //    }
        //}
    }

    /*
    [Obsolete("for class diagram generation only")]
    public abstract class DemoRpcServer: RpcServerBase<DemoInputMessage, DemoOutputMessage>
    {
        public DemoRpcServer(string queuename) : base(queuename)
        {

        }
    }
    */
}
