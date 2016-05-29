using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Orleankka.Core
{
    using Utility;
    using Streams;

    class ActorType : IEquatable<ActorType>
    {
        static readonly Dictionary<string, ActorType> codes =
                    new Dictionary<string, ActorType>();

        static readonly Dictionary<Type, ActorType> types =
                    new Dictionary<Type, ActorType>();

        public static void Register(Assembly[] assemblies)
        {
            var actors = ActorDeclaration.Generate(assemblies);

            foreach (var actor in actors)
                Register(actor);
        }

        static void Register(ActorType actor)
        {
            var registered = codes.Find(actor.Code);
            if (registered != null)
                throw new ArgumentException(
                    $"An actor with {actor.Code} has been already registered");

            codes.Add(actor.Code, actor);
            types.Add(actor.Interface.Type, actor);

            StreamSubscriptionMatcher.Register(actor);
        }

        public static void Reset()
        {
            codes.Clear();
            types.Clear();

            StreamSubscriptionMatcher.Reset();
        }

        public readonly string Code;
        public readonly ActorInterface Interface;
        public readonly ActorImplementation Implementation;

        ActorType(string code, ActorInterface @interface, ActorImplementation implementation)
        {
            Code = code;
            Interface = @interface;
            Implementation = implementation;
        }

        public static ActorType Registered(string code)
        {
            var result = codes.Find(code);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map code '{code}' to the corresponding actor type. " +
                     "Make sure that you've registered the assembly containing this type");

            return result;
        }

        internal static ActorType From(string code, Type interfaceType, Type implementationType)
        {
            var @interface = ActorInterface.From(interfaceType);

            var implementation = implementationType != null
                ? ActorImplementation.From(implementationType)
                : ActorImplementation.Undefined;

            return new ActorType(code, @interface, implementation);
        }

        public bool Equals(ActorType other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(Code, other.Code));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj.GetType() == GetType() && Equals((ActorType) obj));
        }

        public static bool operator ==(ActorType left, ActorType right) => Equals(left, right);
        public static bool operator !=(ActorType left, ActorType right) => !Equals(left, right);

        public override int GetHashCode() => Code.GetHashCode();
        public override string ToString() => Code;
    }

    static class ActorTypeActorSystemExtensions
    {
        internal static ActorRef ActorOf(this IActorSystem system, ActorType type, string id)
        {
            return system.ActorOf(ActorPath.From(type.Code, id));
        }
    }
}