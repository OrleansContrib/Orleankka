using System;
using System.Linq;

namespace Orleankka.Core
{
    static class MessageEnvelope
    {
        internal static IMessageSerializer Serializer;

        static MessageEnvelope()
        {
            Reset();
        }

        internal static void Reset()
        {
            Serializer = new BinarySerializer();
        }
    }
}