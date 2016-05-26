using System;

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

            if (type == typeof(StreamRef))
                return StreamRef.Deserialize(path);

            throw new InvalidOperationException("Unknown ref type: " + type);
        }

        public abstract string Serialize();
    }
}