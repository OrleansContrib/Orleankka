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
                InitializeActorSystemClient();
                InitializeHubClient();
            }

            await Init(publishers);

            return View("Observe");
        }

        static void InitializeActorSystemClient()
        {
            var config = new ClientConfiguration()
                .LoadFromEmbeddedResource<Startup>("Orleans.xml");

            config.DeploymentId = RoleEnvironment.DeploymentId;
            config.DataConnectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");
            config.GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable;

            MvcApplication.System = ActorSystem.Configure().Azure()
                .Client()
                .From(config)
                .Register(typeof(Publisher).Assembly)
                .Done();
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