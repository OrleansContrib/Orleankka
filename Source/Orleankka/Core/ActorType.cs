using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Orleankka.Core
{
    using Utility;
    using Streams;
    using Endpoints;

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
            RegisterEndpoints(assembly);
            RegisterInterfaces(assembly);
            RegisterActors(assembly);
        }

        static void RegisterEndpoints(Assembly assembly)
        {
            var endpoints = assembly.GetTypes()
                .Where(SatisfiesEndpointDeclaration)
                .ToList();

            endpoints.ForEach(RegisterEndpoint);
        }

        static void RegisterInterfaces(Assembly assembly)
        {
            var interfaces = assembly.GetTypes()
                .Where(SatisfiesInterfaceDeclaration)
                .ToList();

            interfaces.ForEach(RegisterInterface);
        }

        static void RegisterActors(Assembly assembly)
        {
            var actors = assembly.GetTypes()
                .Where(SatsfiesActorDeclaration)
                .ToList();

            actors.ForEach(RegisterActor);
        }

        static void RegisterEndpoint(Type type)
        {
            Register(FromEndpoint(type));
        }

        static void RegisterInterface(Type type)
        {
            var actor = FromInterface(type);

            if (codes.ContainsKey(actor.Code))
                return;

            Register(actor);
        }

        static void RegisterActor(Type type)
        {
            var actor = FromActor(type);

            if (types.ContainsKey(actor.ReferenceType()))
                return;

            Register(actor);
        }

        static void Register(ActorType actor)
        {
            var registered = codes.Find(actor.Code);
            if (registered != null)
                throw new ArgumentException(
                    $"The type {actor.ReferenceType()} has been already registered " +
                    $"under the code {registered.Code}");

            codes.Add(actor.Code, actor);
            types.Add(actor.ReferenceType(), actor);

            Ref.Register(actor);
            StreamSubscriptionMatcher.Register(actor);
        }

        public static void Reset()
        {
            codes.Clear();
            types.Clear();

            Ref.Reset();
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

        internal Type ReferenceType() => IsEndpointDeclaration() || IsInterfaceDeclaration()
                                        ? Interface.Type
                                        : Implementation.Type;

        bool IsEndpointDeclaration() => Implementation != ActorImplementation.Undefined 
                                     && Interface.Type != Implementation.Type;

        bool IsInterfaceDeclaration() => Implementation == ActorImplementation.Undefined;

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

        public static ActorType From(Type type)
        {
            if (SatisfiesEndpointDeclaration(type))
                return FromEndpoint(type);

            if (SatisfiesInterfaceDeclaration(type))
                return FromInterface(type);

            if (SatsfiesActorDeclaration(type))
                return FromActor(type);

            throw new InvalidOperationException("Unsupported actor declaration: " + type);
        }

        static bool SatisfiesEndpointDeclaration(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.BaseType != null
                   && type.BaseType.IsConstructedGenericType
                   && type.BaseType.GetGenericTypeDefinition() == typeof(ActorEndpoint<>);
        }

        static bool SatisfiesInterfaceDeclaration(Type type)
        {
            return type.IsInterface
                   && typeof(IActorEndpoint).IsAssignableFrom(type)
                   && !typeof(IFixedEndpoint).IsAssignableFrom(type); // skip built-in;
        }

        static bool SatsfiesActorDeclaration(Type type)
        {
            return !type.IsAbstract && typeof(Actor).IsAssignableFrom(type);
        }

        static ActorType FromEndpoint(Type type)
        {
            Debug.Assert(type.BaseType != null);

            if (type.BaseType.GetGenericTypeDefinition() != typeof(ActorEndpoint<>))
                throw new Exception($"Custom actor endpoint {type} doesn't directly inherit from ActorEndpoint<TActor>");

            var interfaces = type.GetInterfaces()
                .Where(x => typeof(IActorEndpoint).IsAssignableFrom(x))
                .Where(x => x != typeof(IActorEndpoint))
                .ToList();

            if (interfaces.Count == 0)
                throw new Exception($"Custom actor endpoint {type} doesn't declare endpoint interface");

            if (interfaces.Count > 1)
                throw new Exception($"Custom actor endpoint {type} declares more than one endpoint interface");

            Debug.Assert(type.BaseType != null);

            var code = TypeCode(interfaces[0]);
            var interfaceType = interfaces[0];
            var implementationType = type.BaseType.GenericTypeArguments[0];

            return From(code, interfaceType, implementationType);
        }

        static ActorType FromInterface(Type type) => From(TypeCode(type), type, null);
        static ActorType FromActor(Type type) => From(TypeCode(type), type, type);

        static string TypeCode(Type type)
        {
            var customAttribute = type
                .GetCustomAttributes(typeof(ActorTypeCodeAttribute), false)
                .Cast<ActorTypeCodeAttribute>()
                .SingleOrDefault();

            return customAttribute != null
                    ? customAttribute.Code
                    : type.FullName;
        }

        static ActorType From(string code, Type interfaceType, Type implementationType)
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