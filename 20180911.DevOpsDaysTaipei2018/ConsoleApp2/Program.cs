using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main1(string[] args)
        {
            List<Uri> _serverUrls = new List<Uri>();
            var consulClient = new ConsulClient(c => c.Address = new Uri("http://127.0.0.1:8500"));
            var services = consulClient.Agent.Services().Result.Response;
            foreach (var service in services)
            {
                var isSchoolApi = service.Value.Tags.Any(t => t == "School") &&
                                  service.Value.Tags.Any(t => t == "Students");
                if (isSchoolApi)
                {
                    var serviceUri = new Uri($"{service.Value.Address}:{service.Value.Port}");
                    _serverUrls.Add(serviceUri);
                }
            }
        }

        static Random _random = new Random();

        static void Main2(string[] args)
        {
            List<Uri> _serverUrls = new List<Uri>();
            var consulClient = new ConsulClient(c => c.Address = new Uri("http://127.0.0.1:8500"));

            var instances = consulClient.Health.Service("orders_api").Result.Response;

            if (instances.Count() == 0)
            {
                // no service instance found
            }
            else
            {
                var available_instances = (
                    from api in instances
                    where api.Checks.AggregatedStatus().Status == "passing" && api.Service.Tags.Contains("green")
                    select api).ToArray();

                // TODO: ¾Ü¤@©I¥s
                var instance = available_instances[_random.Next(available_instances.Length)];
                var serviceUri = new Uri($"{instance.Service.Address}:{instance.Service.Port}");
                // call serviceUri ...
            }



            var services = consulClient.Agent.Services().Result.Response;

            foreach(var service in (from api in services where api.Value.Tags.Contains("green") select api))
            {
                var serviceUri = new Uri($"{service.Value.Address}:{service.Value.Port}");
                _serverUrls.Add(serviceUri);
            }

            // TODO: call api in {_serverUrls}.
        }
    }
}
