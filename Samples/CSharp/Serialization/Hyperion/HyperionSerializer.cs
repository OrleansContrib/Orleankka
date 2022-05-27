using System;
using System.IO;

using Hyperion;

using Orleankka;
using Orleankka.Meta;
using Orleans.Serialization;

namespace Example
{
    using Microsoft.Extensions.DependencyInjection;

    public class HyperionSerializer : IExternalSerializer
    {
        static readonly Type BaseInterfaceType = typeof(Message);

        readonly Hyperion.Serializer serializer;
        readonly Hyperion.Serializer copier;

        IActorSystem system;

        public HyperionSerializer()
        {
            var surogates = new[]
            {
                Surrogate.Create<ActorPath, ActorPathSurrogate>(ActorPathSurrogate.From, x => x.Original()),
                Surrogate.Create<StreamPath, StreamPathSurrogate>(StreamPathSurrogate.From, x => x.Original()),
                Surrogate.Create<ActorRef, ActorRefSurrogate>(ActorRefSurrogate.From, x => x.Original(this)),
                Surrogate.Create<StreamRef<Item>, StreamRefSurrogate>(StreamRefSurrogate.From, x => x.Original<Item>(this)),
                Surrogate.Create<ClientRef, ClientRefSurrogate>(ClientRefSurrogate.From, x => x.Original(this)),
            };

            var options = SerializerOptions.Default
                .WithVersionTolerance(true)
                .WithPreserveObjectReferences(true)
                .WithSurrogates(surogates);

            serializer = new Hyperion.Serializer(options);

            options = SerializerOptions.Default
                .WithVersionTolerance(false)
                .WithPreserveObjectReferences(true)
                .WithSurrogates(surogates);

            copier = new Hyperion.Serializer(options);
        }

        public bool IsSupportedType(Type itemType)
        {
            return BaseInterfaceType.IsAssignableFrom(itemType);
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            EnsureSystem(context);

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
            EnsureSystem(context);

            var reader = context.StreamReader;

            var length = reader.ReadInt();
            var inBytes = reader.ReadBytes(length);

            using (var stream = new MemoryStream(inBytes))
                return serializer.Deserialize(stream);
        }

        void EnsureSystem(ISerializerContext context)
        {
            if (system == null)
                system = context.ServiceProvider.GetRequiredService<IActorSystem>();
        }

        abstract class StringPayloadSurrogate
        {
            public string S;
        }

        class ActorPathSurrogate : StringPayloadSurrogate
        {
            public static ActorPathSurrogate From(ActorPath path) =>
                new ActorPathSurrogate { S = path };

            public ActorPath Original() => ActorPath.Parse(S);
        }

        class StreamPathSurrogate : StringPayloadSurrogate
        {
            public static StreamPathSurrogate From(StreamPath path) =>
                new StreamPathSurrogate { S = path };

            public StreamPath Original() => StreamPath.Parse(S);
        }

        class ActorRefSurrogate : StringPayloadSurrogate
        {
            public static ActorRefSurrogate From(ActorRef @ref) =>
                new ActorRefSurrogate {S = @ref.Path};

            public ActorRef Original(HyperionSerializer ctx) => 
                ctx.system.ActorOf(ActorPath.Parse(S));
        }

        class StreamRefSurrogate : StringPayloadSurrogate
        {
            public static StreamRefSurrogate From<TItem>(StreamRef<TItem> @ref) =>
                new StreamRefSurrogate { S = @ref.Path };

            public StreamRef<TItem> Original<TItem>(HyperionSerializer ctx) =>
                ctx.system.StreamOf<TItem>(StreamPath.Parse(S));
        }

        class ClientRefSurrogate : StringPayloadSurrogate
        {
            public static ClientRefSurrogate From(ClientRef @ref) =>
                new ClientRefSurrogate { S = @ref };

            public ClientRef Original(HyperionSerializer ctx) =>
                ctx.system.ClientOf(S);
        }
    }
}