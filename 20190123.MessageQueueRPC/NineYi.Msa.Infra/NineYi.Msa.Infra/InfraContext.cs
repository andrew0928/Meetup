using System;
using System.Collections.Generic;
using System.Text;

namespace NineYi.Msa.Infra
{


    public class InfraContext
    {
        public string HostId { get; set; }

        public string HostGroup { get; set; }

        /// <summary>
        /// from IHostEnvironment
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// from IHostEnvironment
        /// </summary>
        public string ApplicationName { get; set; }

        public string Market { get; set; }

        public string Region { get; set; }

        [Obsolete]
        public static InfraContext Current { get; set; }

    }
}
