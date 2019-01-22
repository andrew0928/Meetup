using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace NineYi.Msa.Infra.Messaging
{
    [Obsolete]
    public class MessageConfig
    {
        private const string DefaultName = "Connections/Rabbitmq/MessageBus";

        public const int DefaultRetryCount = 30;

        public static readonly TimeSpan DefaultRetryWaitTime = TimeSpan.FromSeconds(3.0);
        public static readonly TimeSpan DefaultPullWaitTime = TimeSpan.FromMilliseconds(30);


        public static readonly TimeSpan DefaultWaitReplyTimeOut = TimeSpan.FromSeconds(15);
        public static readonly TimeSpan DefaultMessageExpirationTimeout = TimeSpan.FromSeconds(30 * 60);

        public static readonly TimeSpan DefaultMessageTTL = TimeSpan.FromSeconds(10);



        [Obsolete]
        public static ConnectionFactory DefaultConnectionFactory
        {
            get
            {
                var fac = GetMessageBusConnectionFactoryFromConfig(DefaultName);
                return fac;
            }
        }


        [Obsolete]
        public static ConnectionFactory GetMessageBusConnectionFactoryFromConfig(string configName)
        {
            //
            // TODO: 應該改到 consul 查詢相關資訊
            //


            return new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
            };



            /*
            //var configItem = ConfigurationManager.ConnectionStrings[configName];
            var configItem = ServiceClient.Instance.GetConfigString(configName);
            if (configItem == null) //|| configItem.ProviderName != "RabbitMQ"
            {
                return null;
            }

            string connstr = configItem;
            ConnectionFactory fac = new ConnectionFactory();
            fac.Port = 5672; // set default port

            fac.AutomaticRecoveryEnabled = false;
            fac.TopologyRecoveryEnabled = false;
            fac.NetworkRecoveryInterval = TimeSpan.FromSeconds(3.0);
            fac.RequestedHeartbeat = 5;

            foreach (string segment in connstr.Split(';'))
            {
                string[] temp = segment.Split('=');
                if (temp.Length != 2) continue;

                string name = temp[0];
                string value = temp[1];

                if (string.IsNullOrEmpty(value)) continue;

                switch (name)
                {
                    case "server":
                        fac.HostName = value;
                        break;

                    case "port":
                        //QueuePortNumber = int.Parse(value);
                        fac.Port = int.Parse(value);
                        break;

                    case "username":
                        fac.UserName = value;
                        break;

                    case "password":
                        fac.Password = value;
                        break;
                }
            }

            

            return fac;
            */
        }



        //        public static string QueueHostName = null;//"172.19.3.143";
        //        public static int QueuePortNumber = 5672;






        //        static QueueConfig()
        //        {
        //            //MessageFilter = new MessagePropertyFilter();
        //            //MessageFilter.SetAll();
        //            //MessageFilter.DefaultBodySize = 1024 * 1024;





        //            if (ConfigurationManager.ConnectionStrings["MessageBus"] != null && ConfigurationManager.ConnectionStrings["MessageBus"].ProviderName == "RabbitMQ")
        //            {
        //                string connstr = ConfigurationManager.ConnectionStrings["MessageBus"].ConnectionString;

        //                foreach (string segment in connstr.Split(';'))
        //                {
        //                    string[] temp = segment.Split('=');
        //                    if (temp.Length != 2) continue;

        //                    string name = temp[0];
        //                    string value = temp[1];

        //                    switch(name)
        //                    {
        //                        case "server":
        //                            QueueHostName = value;
        //                            break;

        //                        case "port":
        //                            QueuePortNumber = int.Parse(value);
        //                            break;

        //                        case "username":
        //                        case "password":
        //                            break;
        //                    }
        //                }
        //            }





        //        }
    }

}
