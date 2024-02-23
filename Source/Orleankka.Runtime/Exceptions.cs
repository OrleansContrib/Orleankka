﻿using System;
using System.Runtime.Serialization;

using Orleans;

namespace Orleankka
{
    [GenerateSerializer, Serializable]
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(ActorGrain actor, object message, string details = "")
            : base($"An actor '{actor.GetType()}::{actor.Id}' cannot handle '{message.GetType()}'{details}")
        {}
    }
}