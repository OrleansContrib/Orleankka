using System;

using Orleankka.Core;

namespace Orleankka
{
    public abstract class OrleankkaException : Exception 
    {
        protected OrleankkaException(string message, Exception innerException = null)
            : base(message, innerException)
        {}
    }

    public class DuplicateActorTypeException : Exception 
    {
        internal DuplicateActorTypeException(ActorInterfaceMapping existing, ActorInterfaceMapping duplicate)
            : base($"Type {duplicate.Types[0]} specifies '{existing.FullName}' actor type code or implements same custom interface " +
                   $"which has been already registered for type {existing.Types[0]}")
        {}
    }
}