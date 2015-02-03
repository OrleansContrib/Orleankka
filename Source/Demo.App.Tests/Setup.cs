using System;
using System.Linq;

using NUnit.Framework;

using Demo;
using Orleankka;
using Orleankka.TestKit;

[assembly: Setup]

namespace Demo
{
    public class SetupAttribute : TestActionAttribute
    {
        public override void BeforeTest(TestDetails details)
        {
            if (!details.IsSuite)
                return;

            ActorSystem.Register(typeof(Topic).Assembly);
            ActorSystem.Register(typeof(ActorRefStub).Assembly);
        }
    }
}
