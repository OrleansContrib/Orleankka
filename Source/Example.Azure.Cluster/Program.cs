using System.Net;
using System.Reflection;

using Microsoft.WindowsAzure.ServiceRuntime;

using Orleankka;
using Orleankka.Cluster;
using Orleankka.CSharp;

using Orleans.Runtime.Configuration;

namespace Example.Azure
{
    public class Program : RoleEntryPoint
    {
        IActorSystem system;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            var config = new ClusterConfiguration()
                .LoadFromEmbeddedResource<Program>("Orleans.xml");

            system = ActorSystem.Configure() //.Azure()
                .Cluster()
                .From(config)
                .CSharp(x => x.Register(Assembly.GetExecutingAssembly()))
                .Run<HubGateway.Bootstrapper>()
                .Done();

            return base.OnStart();
        }

        public override void Run()
        {
//            system.Run();
        }
    }
}
