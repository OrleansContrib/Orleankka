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
        public void Serialization()
        {
            // - Add support for native Orleans serializer
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
        public void ActorPrototype()
        {
            // - Allow to override automatic handler wire-up
            // - Allow to specify reentrant message types, via lambda
            // - Prototype extensibility?
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
            // - Add support (deep-copy) for mutable (yay) messages
            // - Support weak (naming convention) attribute definition, 
            //   to remove dependency on Orleankka from message contract lib ???
        }
    }
}
