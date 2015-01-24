using System;

using Orleans;
using Orleankka;

using Microsoft.WindowsAzure.Storage;

namespace Demo
{
    public static class Program
    {
        static OrleansSilo silo;
        static Client client;

        public static void Main()
        {
            var args = new[]
            {
                "UseDevelopmentStorage=true" // # TopicStorageAccount
            };

            var hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = StartSilo,
                AppDomainInitializerArguments = args,
            });

            OrleansClient.Initialize("ClientConfiguration.xml");
            RunClient();

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            hostDomain.DoCallBack(StopSilo);
        }

        static void StartSilo(string[] args)
        {
            TopicStorage.Init(CloudStorageAccount.Parse(args[0]));

            silo = new OrleansSilo();
            silo.Start();
        }

        static void StopSilo()
        {
            silo.Stop();
        }

        static void RunClient()
        {
            client = new Client(ActorSystem.Instance, ActorObserverProxy.Create().Result);
            client.Run();
        }
    }
}
