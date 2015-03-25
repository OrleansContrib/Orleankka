using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Testing
{
    using Playground;

    public class RequiresSiloAttribute : TestActionAttribute
    {
        public bool Fresh                     {get; set;}
        public int? GCTimeoutInMilliseconds   {get; set;}

        public override void BeforeTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;
            
            if (Fresh)
                TeardownExisting();

            StartNew();
        }

        static void TeardownExisting()
        {
            if (TestActorSystem.Instance == null)
                return;

            TestActorSystem.Instance.Dispose();
            TestActorSystem.Instance = null;
        }

        void StartNew()
        {
            if (TestActorSystem.Instance != null)
                return;

            var configurator = ActorSystem.Configure()
                .Playground()
                .Register(GetType().Assembly)
                .Serializer<JsonSerializer>();

            if (GCTimeoutInMilliseconds != null)
            {
                var timeout = TimeSpan.FromMilliseconds(GCTimeoutInMilliseconds.Value);
                configurator.Cluster.Globals.Application.SetDefaultCollectionAgeLimit(timeout);
            }

            TestActorSystem.Instance = configurator.Done();
        }
    }
}
