using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Orleankka
{
    using Dynamic;
    using Dynamic.Internal;

    public sealed partial class ActorSystem
    {
        /// <summary>
        /// Global configuration options for dynamic actor feature.
        /// </summary>
        public static class Dynamic
        {
            static Dynamic()
            {
                Activator = path => (DynamicActor) System.Activator.CreateInstance(path.Type);

                Serializer = obj =>
                {
                    using (var ms = new MemoryStream())
                    {
                        new BinaryFormatter().Serialize(ms, obj);
                        return ms.ToArray();
                    }
                };

                Deserializer = bytes =>
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        var formatter = new BinaryFormatter();
                        return formatter.Deserialize(ms);
                    }
                };
            }

            /// <summary>
            /// The activation function, which creates actual instances of <see cref="DynamicActor"/>
            /// </summary>
            /// <remarks>
            /// By default expects type to have a public parameterless constructor 
            /// as a consequence of using standard  <see cref="System.Activator"/>
            /// </remarks>
            public static Func<ActorPath, DynamicActor> Activator { get; set; }

            /// <summary>
            /// The serialization function, which serializes messages to byte[]
            /// </summary>
            /// <remarks>
            /// By default uses standard binary serialization provided by <see cref="BinaryFormatter"/>
            /// </remarks>
            public static Func<object, byte[]> Serializer
            {
                get { return DynamicMessage.Serializer; }
                set { DynamicMessage.Serializer = value; }
            }

            /// <summary>
            /// The deserialization function, which deserializes byte[] back to messages
            /// </summary>
            /// <remarks>
            /// By default uses standard binary serialization provided by 
            /// <see cref="BinaryFormatter"/></remarks>
            public static Func<byte[], object> Deserializer
            {
                get { return DynamicMessage.Deserializer; }
                set { DynamicMessage.Deserializer = value; }
            }

            internal static readonly IActorSystem Instance = new DynamicActorSystem(Orleankka.ActorSystem.Instance);

            class DynamicActorSystem : IActorSystem
            {
                readonly IActorSystem system;

                public DynamicActorSystem(IActorSystem system)
                {
                    this.system = system;
                }

                public ActorRef ActorOf(ActorPath path)
                {
                    return system.ActorOf(path);
                }

                public IActorObserver ObserverOf(ActorPath path)
                {
                    return ClientObservable.IsCompatible(path) 
                            ? new DynamicActorObserver(ClientObservable.DynamicObserver(path)) 
                            : system.ObserverOf(path);
                }
            }
        }
    }
}