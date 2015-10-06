using System;
using System.Diagnostics;
using System.Reflection;

namespace Orleankka
{
    public abstract class Ref
    {
        public static Ref Deserialize(string path, Type type)
        {
            if (type == typeof(ClientRef))
                return ClientRef.Deserialize(path);

            if (type == typeof(ActorRef))
                return ActorRef.Deserialize(path);

            if (type == typeof(ActorRef<>))
            {
                var @ref = ActorRef.Deserialize(path);
                var constructor = type.GetConstructors(BindingFlags.NonPublic)[0];
                Debug.Assert(constructor != null);
                return (Ref) constructor.Invoke(new object[] {@ref});
            }

            if (type == typeof(StreamRef))
                return ActorRef.Deserialize(path);

            throw new InvalidOperationException("Unknown ref type: " + type);
        }

        public abstract string Serialize();
    }
}