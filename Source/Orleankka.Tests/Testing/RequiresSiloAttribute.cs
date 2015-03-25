using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Testing
{
    using Playground;

    public class RequiresSiloAttribute : TestActionAttribute
    {
        public override void BeforeTest(TestDetails details)
        {
            if (TestActorSystem.Instance != null)
                return;

            TestActorSystem.Instance = ActorSystem.Configure()
                .Playground()
                .Register(GetType().Assembly)
                .Serializer<JsonSerializer>()
                .Done();
        }
    }
}
