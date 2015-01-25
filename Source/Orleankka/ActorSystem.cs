using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using Orleans.Providers;

namespace Orleankka
{
    /// <summary>
    /// Serves as factory for acquiring actor/observer references from their paths.
    /// </summary>
    public interface IActorSystem
    {
        /// <summary>
        /// Acquires the reference for the given actor path.
        /// </summary>
        /// <param name="path">The actor path</param>
        /// <returns>An actor reference</returns>
        ActorRef ActorOf(ActorPath path);

        /// <summary>
        /// Acquires the reference to <see cref="IActorObserver"/> from its <see cref="ActorPath"/>.
        /// </summary>
        /// <param name="path">The path of actor observer.</param>
        /// <returns>The instance of <see cref="IActorObserver"/> </returns>
        IActorObserver ObserverOf(ActorPath path);
    }

    /// <summary>
    /// The actor system extensions.
    /// </summary>
    public static class ActorSystemExtensions
    {
        /// <summary>
        /// Acquires the reference for the given id and type of the actor.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor</typeparam>
        /// <param name="system">The reference to actor system</param>
        /// <param name="id">The id</param>
        /// <returns>An actor reference</returns>
        public static ActorRef ActorOf<TActor>(this IActorSystem system, string id)
        {
            return system.ActorOf(new ActorPath(typeof(TActor), id));
        }        
    }

    /// <summary>
    /// Default implementation of <see cref="IActorSystem"/>
    /// </summary>
    public sealed class ActorSystem : IActorSystem
    {
        /// <summary>
        /// The static instance of <see cref="IActorSystem"/>
        /// </summary>
        public static readonly IActorSystem Instance = new ActorSystem();

        ActorSystem()
        {}

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            Requires.NotNull(path, "path");

            if (Actor.IsCompatible(path.Type))
                return new ActorRef(path, Actor.Proxy(path));

            if (DynamicActor.IsCompatible(path.Type))
                return new ActorRef(path, DynamicActor.Proxy(path));

            throw new ArgumentException("Path type should be either an interface which implements IActor or non-abstract type inherited from DynamicActor", "path");
        }
        
        IActorObserver IActorSystem.ObserverOf(ActorPath path)
        {
            Requires.NotNull(path, "path");

            if (ClientObservable.IsCompatible(path))
                return ClientObservable.Observer(path);

            if (Actor.IsCompatible(path.Type))
                return Actor.Observer(path);

            if (DynamicActor.IsCompatible(path.Type))
                return DynamicActor.Observer(path);

            throw new InvalidOperationException("Can't bind " + path.Type);
        }

        public static class Dynamic
        {
            /// <summary>
            /// The activation function, which creates actual instances of <see cref="DynamicActor"/>
            /// </summary>
            public static Func<Orleankka.ActorPath, DynamicActor> Activator = path => 
                (DynamicActor) System.Activator.CreateInstance(path.Type);

            /// <summary>
            /// The serialization function, which serializes messages to byte[]
            /// </summary>
            public static Func<object, byte[]> Serializer = message =>
            {
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, message);
                    return ms.ToArray();
                }
            };

            /// <summary>
            /// The deserialization function, which deserializes byte[] back to messages
            /// </summary>
            public static Func<byte[], object> Deserializer = message =>
            {
                using (var ms = new MemoryStream(message))
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(ms);
                }
            };

            public static class ActorPath
            {
                static readonly string[] separator = {"::"};

                /// <summary>
                /// The serialization function, which serializes <see cref="Orleankka.ActorPath"/> to runtime identity string
                /// </summary>
                public static Func<Orleankka.ActorPath, string> Serializer = path => 
                    string.Format("{0}{1}{2}", path.Type.FullName, separator[0], path.Id);

                /// <summary>
                /// The deserialization function, which deserializes runtime identity string back to <see cref="Orleankka.ActorPath"/>
                /// </summary>
                public static Func<string, Orleankka.ActorPath> Deserializer = path =>
                {
                    var parts = path.Split(separator, 2, StringSplitOptions.None);
                    return new Orleankka.ActorPath(Type.GetType(parts[0]), parts[1]);
                };
            }
        }

        public abstract class Bootstrapper : IBootstrapProvider
        {
            public string Name {get; private set;}

            Task IOrleansProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
            {
                Name = name;
                return Init(config.Properties);
            }

            public abstract Task Init(IDictionary<string, string> properties);
        }
    }
}