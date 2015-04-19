using System;
using System.Linq;

namespace Orleankka
{
    public abstract class Ref
    {
        public static Ref Deserialize(string path)
        {
            if (ClientRef.Satisfies(path))
                return ClientRef.Deserialize(path);

            return ActorRef.Deserialize(ActorPath.Deserialize(path));
        }

        public abstract string Serialize();
    }
}