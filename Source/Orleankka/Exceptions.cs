using System;

namespace Orleankka
{
    using Core;

    public abstract class OrleankkaException : Exception 
    {
        protected OrleankkaException(string message, Exception innerException = null)
            : base(message, innerException)
        {}
    }

    public class DuplicateActorTypeException : OrleankkaException 
    {
        internal DuplicateActorTypeException(ActorInterfaceMapping existing, ActorInterfaceMapping duplicate)
            : base($"Type {duplicate.Types[0]} specifies '{existing.TypeName}' actor type code or implements same custom interface " +
                   $"which has been already registered for type {existing.Types[0]}")
        {}
    }
}