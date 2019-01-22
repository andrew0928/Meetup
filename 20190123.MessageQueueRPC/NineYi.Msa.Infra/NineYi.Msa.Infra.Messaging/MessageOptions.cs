using System;
using System.Collections.Generic;
using System.Text;

namespace NineYi.Msa.Infra.Messaging
{
    public abstract class MessageOptions
    {
        public string ConnectionName = "default-connection";

        [Obsolete("Use ConnectionURL instead.")]
        public string HostName = "localhost";

        [Obsolete("Use ConnectionURL instead.")]
        public int Port = 5672;

        [Obsolete("Use ConnectionURL instead.")]
        public string VirtualHost = "/";

        [Obsolete("Use ConnectionURL instead.")]
        public string UserName = "guest";

        [Obsolete("Use ConnectionURL instead.")]
        public string Password = "guest";

        // format: amqp://{username}:{password}@{hostname}:{port}/{vhost}
        //public string ConnectionURL = "";
        public string ConnectionURL { get; set; } = @"amqp://guest:guest@localhost:5672/";
        //{
        //    get
        //    {
        //        //return new Uri($"amqp://{this.UserName}:{this.Password}@{this.HostName}:{this.Port}{this.VirtualHost}");
        //        if (string.IsNullOrWhiteSpace(this._connectionURL))
        //        {
        //            return $"amqp://{this.UserName}:{this.Password}@{this.HostName}:{this.Port}{this.VirtualHost}";
        //        }
        //        return _connectionURL;
        //    }
        //    set
        //    {
        //        this._connectionURL = value;
        //    }
        //}
        //private string _connectionURL = null;

        public int ConnectionRetryCount = 3;
        public TimeSpan ConnectionRetryTimeout = TimeSpan.FromSeconds(30);
    }

}
