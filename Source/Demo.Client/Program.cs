using System;
using System.Collections.Generic;

using Orleankka;
using Orleans.Runtime.Configuration;

namespace Demo
{
    public static class Program
    {
        static Client client;

        public static void Main()
        {
            var serverConfig = new ServerConfiguration()
                .LoadFromEmbeddedResource<Client>("Orleans.Server.Configuration.xml");

            var clientConfig = new ClientConfiguration()
                .LoadFromEmbeddedResource<Client>("Orleans.Client.Configuration.xml");

            var properties = new Dictionary<string, string> 
              {{"account", "UseDevelopmentStorage=true"}};

            var silo = new EmbeddedSilo()
                .With(serverConfig)
                .With(clientConfig)
                .Use<ServiceLocator>(properties)
                .Register(typeof(Api).Assembly)
                .Start();

            client = new Client(ActorSystem.Instance, Observer.Create().Result);
            client.Run();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            silo.Dispose();
        }
    }
}
