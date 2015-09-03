using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Core
{
    using Utility;

    class ActorType : IEquatable<ActorType>
    {
        static readonly Dictionary<string, ActorType> codes =
                    new Dictionary<string, ActorType>();

        static readonly Dictionary<Type, ActorType> types =
                    new Dictionary<Type, ActorType>();

        public static void Register(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                Register(assembly);
        }

        static void Register(Assembly assembly)
        {
            var actors = assembly
                .GetTypes()
                .Where(x =>
                       !x.IsAbstract
                       && typeof(Actor).IsAssignableFrom(x));

            foreach (var actor in actors)
                Register(actor);
        }

        static void Register(Type actor)
        {
            var type = RegisterThis(actor);

            ActorInterface.Register(type);
            ActorPrototype.Register(type);

            ActorEndpointFactory.Register(type);
        }

        static ActorType RegisterThis(Type actor)
        {
            var type = ActorType.Of(actor);
            var registered = codes.Find(type.Code);

            if (registered != null)
                throw new ArgumentException(
                    $"The type {actor} has been already registered " +
                    $"under the code {registered.Code}");

            codes.Add(type.Code, type);
            types.Add(actor, type);

            return type;
        }

        public static void Reset()
        {
            ResetThis();

            ActorInterface.Reset();
            ActorPrototype.Reset();

            ActorEndpointFactory.Reset();
        }

        static void ResetThis()
        {
            codes.Clear();
            types.Clear();
        }

        public readonly string Code;
        public readonly Type Interface;
        public readonly Type Implementation;

        ActorType(string code, Type @interface, Type implementation)
        {
            Code = code;
            Interface = @interface;
            Implementation = implementation;
        }

        public static ActorType Registered(Type type)
        {
            var result = types.Find(type);

            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map type '{type}' to the corresponding actor type. " +
                     "Make sure that you've registered the assembly containing this type");

            return result;
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

        public static ActorType Of(Type type)
        {
            var customAttribute = type
                .GetCustomAttributes(typeof(ActorTypeCodeAttribute), false)
                .Cast<ActorTypeCodeAttribute>()
                .SingleOrDefault();

            var code = customAttribute != null
                        ? customAttribute.Code
                        : type.FullName;

            return new ActorType(code, type, type);
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

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public static bool operator ==(ActorType left, ActorType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActorType left, ActorType right)
        {
            return !Equals(left, right);
        }
    }
}