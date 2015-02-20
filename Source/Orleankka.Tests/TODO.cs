using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class TodoFixture
    {
        [Test, Ignore]
        public void Envelopes()
        {
            // - Envelope and Body attributte
            // - Proper selection of Receive channel based on Body type
            // - Proper dispatching in TypeActor based on Body type
        }

        [Test, Ignore]
        public void AutomaticDeactivation()
        {
            // - Add support for per-type idle deactivation timeouts
        }

        [Test, Ignore]
        public void AzureSystem()
        {
            // - Finish actor system configuration
        }

        [Test, Ignore]
        public void TypedActors()
        {
            // - TypedActor with automatic handler wire-up and ability to override dispatch (On, Via)
        }

        [Test, Ignore]
        public void Samples()
        {
            // - Add DI container sample (Unity)
            // - Add ProtoBuf/Bond serialization sample
            // - Add Azure CloudService sample
        }

        [Test, Ignore]
        public void Messages()
        {
            // - Require [Message] attribute for all in/out message classes ???
            // - Add support (deep-copy) for mutable (yay) messages
            // - Support weak (name convention) attribute definition, 
            //   to remove dependency on Orleankka from message contract lib ???
        }
    }
}
