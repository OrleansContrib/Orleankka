using System;
using System.IO;
using System.Runtime.Serialization;

using Hyperion;

using Orleankka;
using Orleankka.Meta;

using Orleans;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streams;

namespace Example
{
    public class HyperionSerializer : IExternalSerializer
    {
        static readonly Type BaseInterfaceType = typeof(Message);

        readonly Serializer serializer;
        readonly Serializer copier;

        IStreamProviderManager streamProviderManager;
        IGrainFactory grainFactory;

        HyperionSerializer()
        {
            var surogates = new[]
            {
                Surrogate.Create<ActorPath, ActorPathSurrogate>(ActorPathSurrogate.From, x => x.Original()),
                Surrogate.Create<ActorRef, ActorRefSurrogate>(ActorRefSurrogate.From, x => x.Original(this)),
                Surrogate.Create<StreamPath, StreamPathSurrogate>(StreamPathSurrogate.From, x => x.Original()),
                Surrogate.Create<StreamRef, StreamRefSurrogate>(StreamRefSurrogate.From, x => x.Original(this)),
                Surrogate.Create<ClientRef, ClientRefSurrogate>(ClientRefSurrogate.From, x => x.Original(this)),
            };

            var options = new SerializerOptions(
                versionTolerance: true, 
                preserveObjectReferences: true, 
                surrogates: surogates);

            serializer = new Serializer(options);

            options = new SerializerOptions(
                versionTolerance: false,
                preserveObjectReferences: true, 
                surrogates: surogates);

            copier = new Serializer(options);
        }

        public void Initialize(Logger logger)
        {}

        public bool IsSupportedType(Type itemType)
        {
            return BaseInterfaceType.IsAssignableFrom(itemType);
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            if (source == null)
                return null;

            using (var stream = new MemoryStream())
            {
                copier.Serialize(source, stream);
                stream.Position = 0;
                return copier.Deserialize(stream);
            }
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {
            var writer = context.StreamWriter;

            if (item == null)
            {
                writer.WriteNull();
                return;
            }

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(item, stream);
                var outBytes = stream.ToArray();
                writer.Write(outBytes.Length);
                writer.Write(outBytes);
            }
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            if (streamProviderManager == null)
                streamProviderManager = (IStreamProviderManager)context.ServiceProvider.GetService(typeof(IStreamProviderManager));

            if (grainFactory == null)
                grainFactory = (IGrainFactory)context.ServiceProvider.GetService(typeof(IGrainFactory));

            var reader = context.StreamReader;

            var length = reader.ReadInt();
            var inBytes = reader.ReadBytes(length);

            using (var stream = new MemoryStream(inBytes))
                return serializer.Deserialize(stream);
        }

        abstract class StringPayloadSurrogate
        {
            public string S;
        }

        class ActorPathSurrogate : StringPayloadSurrogate
        {
            public static ActorPathSurrogate From(ActorPath path) =>
                new ActorPathSurrogate { S = path.Serialize() };

            public ActorPath Original() => ActorPath.Deserialize(S);
        }

        class ActorRefSurrogate : StringPayloadSurrogate
        {
            public static ActorRefSurrogate From(ActorRef @ref) =>
                new ActorRefSurrogate { S = @ref.Path.ToString()};

            public ActorRef Original(HyperionSerializer ctx) => 
                ActorRef.Deserialize(ActorPath.Deserialize(S), ctx.grainFactory);
        }

        class StreamPathSurrogate : StringPayloadSurrogate
        {
            public static StreamPathSurrogate From(StreamPath path) =>
                new StreamPathSurrogate { S = path.Serialize() };

            public StreamPath Original() => StreamPath.Deserialize(S);
        }

        class StreamRefSurrogate : StringPayloadSurrogate
        {
            public static StreamRefSurrogate From(StreamRef @ref) =>
                new StreamRefSurrogate { S = @ref.Serialize() };

            public StreamRef Original(HyperionSerializer ctx) => 
                StreamRef.Deserialize(StreamPath.Deserialize(S), ctx.streamProviderManager);
        }

        class ClientRefSurrogate : StringPayloadSurrogate
        {
            public static ClientRefSurrogate From(ClientRef @ref) =>
                new ClientRefSurrogate { S = @ref.Path };

            public ClientRef Original(HyperionSerializer ctx) => 
                ClientRef.Deserialize(S);
        }
    }
}