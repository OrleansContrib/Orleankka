using System;

using Orleans.Serialization;

namespace Orleankka.Testing
{
    using System.Reflection;

    using Meta;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    public class MessageSerializer : IExternalSerializer
    {
        readonly Lazy<OrleansJsonSerializer> serializer;

        public MessageSerializer(IServiceProvider services)
        {
            serializer = new Lazy<OrleansJsonSerializer>(() =>
            {
                var system = services.GetRequiredService<IActorSystem>();
                var sr = new OrleansJsonSerializer(services);
                var settings = (Lazy<JsonSerializerSettings>) sr.GetType().GetField("settings", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(sr);
                settings!.Value.Converters.Add(new ObserverRefConverter(system));
                settings!.Value.Converters.Add(new ClientRefConverter(system));
                settings!.Value.Converters.Add(new ActorRefConverter(system));
                settings!.Value.Converters.Add(new TypedActorRefConverter(system));
                settings!.Value.Converters.Add(new StreamRefConverter(services));
                settings!.Value.Converters.Add(new StreamFilterConverter());
                return sr;
            });
        }

        public bool IsSupportedType(Type itemType)
        {
            return typeof(Message).IsAssignableFrom(itemType);
        }

        public object DeepCopy(object source, ICopyContext context)
        {
            return serializer.Value.DeepCopy(source, context);
        }

        public void Serialize(object item, ISerializationContext context, Type expectedType)
        {
            serializer.Value.Serialize(item, context, expectedType);
        }

        public object Deserialize(Type expectedType, IDeserializationContext context)
        {
            return serializer.Value.Deserialize(expectedType, context);
        }
    }
}