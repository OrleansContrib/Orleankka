using System;
using System.Linq;

namespace Orleankka.Dynamic.Internal
{
    static class DynamicMessage
    {
        internal static Func<object, byte[]> Serializer;
        internal static Func<byte[], object> Deserializer;
    }
}
