using System;
using System.Linq;
using System.Net;

using NUnit.Framework;

using Orleans;
using Orleans.Host;
using Orleans.Runtime.Configuration;

using Orleankka;
[assembly: EmbeddedSiloAction]

namespace Orleankka
{
    public class EmbeddedSiloActionAttribute : TestActionAttribute
    {
        EmbeddedSilo silo;

        public override void BeforeTest(TestDetails details)
        {
            if (details.IsSuite)
                silo = new EmbeddedSilo();
        }

        public override void AfterTest(TestDetails details)
        {
            if (details.IsSuite)
                silo.Dispose();
        }
    }

    public class EmbeddedSilo : IDisposable
    {
        static OrleansSiloHost host;
        static AppDomain domain;

        public EmbeddedSilo()
        {
            var setup = AppDomain.CurrentDomain.SetupInformation;

            domain = AppDomain.CreateDomain("EmbeddedSilo", null, new AppDomainSetup
            {
                AppDomainInitializer = Start,
                AppDomainInitializerArguments = new string[0],
                ApplicationBase = setup.ApplicationBase,
                CachePath = setup.CachePath,
                ConfigurationFile = setup.ConfigurationFile,
                PrivateBinPath = setup.PrivateBinPath,
                ShadowCopyDirectories = setup.ShadowCopyDirectories,
                ShadowCopyFiles = setup.ShadowCopyFiles
            });

            var clientConfigFileName = ConfigurationFilePath("Orleans.Client.Configuration.xml");
            OrleansClient.Initialize(clientConfigFileName);
        }

        public void Dispose()
        {
            domain.DoCallBack(Shutdown);
            AppDomain.Unload(domain);
        }

        static void Start(string[] args)
        {
            var serverConfigFileName = ConfigurationFilePath("Orleans.Server.Configuration.xml");
            host = new OrleansSiloHost(Dns.GetHostName()) {ConfigFileName = serverConfigFileName};

            host.LoadOrleansConfig();
            host.InitializeOrleansSilo();

            host.Config.Globals.ReminderServiceType =
                GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;

            host.StartOrleansSilo();
        }

        static void Shutdown()
        {
            if (host == null)
                return;

            host.StopOrleansSilo();
            host.Dispose();

            host = null;
        }

        static string ConfigurationFilePath(string configFileName)
        {
            var outputDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return System.IO.Path.Combine(outputDirectory, @"$Testing$\" + configFileName);
        }
    }
}
