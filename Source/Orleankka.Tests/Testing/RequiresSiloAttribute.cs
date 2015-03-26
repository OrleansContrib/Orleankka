using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Testing
{
    using Playground;

    public class RequiresSiloAttribute : TestActionAttribute
    {
        public bool Fresh;

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

            var system = ActorSystem.Configure()
                                    .Playground()
                                    .Register(GetType().Assembly)
                                    .Serializer<JsonSerializer>()
                                    .Done();

            TestActorSystem.Instance = system;
        }
    }
}
