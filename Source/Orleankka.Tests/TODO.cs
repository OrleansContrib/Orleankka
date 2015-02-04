using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class TodoFixture
    {
        [Test, Ignore]
        public void LambdaActors()
        {
            // - Add support for function actors
            // - Need to support all avail attributes (passed inside spawn() func)
        }

        [Test, Ignore]
        public void AutomaticDeactivation()
        {
            // - Add support for per-type idle deactivation timeouts
        }
    }
}
