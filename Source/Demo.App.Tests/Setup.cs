using System;
using System.Linq;

using NUnit.Framework;

using Demo;
using Orleankka.Core;
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
        }
    }
}
