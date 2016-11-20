using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using Orleankka;
using Orleankka.Client;

using Orleans.Runtime.Configuration;

namespace Example.Azure.Controllers
{
    using Hubs;

    using Microsoft.WindowsAzure.ServiceRuntime;

    public class HomeController : Controller
    {
        [HttpGet]
        public ViewResult Observe()
        {
            return View();
        }

        [HttpGet]
        public ViewResult Spawn()
        {
            return View();
        }

        [HttpPost]
        public async Task<ViewResult> Spawn(int publishers)
        {
            if (MvcApplication.System == null)
            {
                InitializeClientActorSystem();
                InitializeHubClient();
            }

            await Init(publishers);

            return View("Observe");
        }

        static void InitializeClientActorSystem()
        {
            var clusterId = RoleEnvironment.DeploymentId;
            var clsuterMembershipStorage = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");

            MvcApplication.System = ActorSystem.Configure()
                .Client()
                .From(Configuration(clusterId, clsuterMembershipStorage))
                .Register(typeof(Publisher).Assembly)
                .Done();

            MvcApplication.System.Connect(retries: 5);
        }

        static ClientConfiguration Configuration(string deploymentId, string dataConnectionString)
        {
            var client = new ClientConfiguration()
                .LoadFromEmbeddedResource<Startup>("Orleans.xml");

            client.DeploymentId = deploymentId;
            client.DataConnectionString = dataConnectionString;
            client.GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable;

            return client;
        }

        static void InitializeHubClient()
        {
            HubClient.Initialize();
        }

        static Task Init(int publishers)
        {
            var activations = new List<Task>();
            
            foreach (var i in Enumerable.Range(1, publishers))
            {
                var activation = MvcApplication.System.ActorOf<Publisher>(i.ToString());
                activations.Add(activation.Tell(new Publisher.Init()));
            }

            return Task.WhenAll(activations.ToArray());
        }
    }
}