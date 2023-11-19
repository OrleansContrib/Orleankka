using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Options;

using Orleankka.Utility;

using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleankka.Legacy.Streams
{
    class StreamSubscriptionTable
    {
        readonly IGrainFactory grainFactory;
        readonly Dictionary<string, List<StreamSubscriptionSpecification>> streamSubscriptions;

        public StreamSubscriptionTable(IOptions<GrainTypeOptions> grainTypeOptions, IGrainFactory grainFactory, IDispatcherRegistry registry)
        {
            Requires.NotNull(grainTypeOptions, nameof(grainTypeOptions));
            Requires.NotNull(grainTypeOptions.Value, nameof(grainTypeOptions.Value));
            Requires.NotNull(grainFactory, nameof(grainFactory));
            Requires.NotNull(registry, nameof(registry));

            this.grainFactory = grainFactory;
            this.streamSubscriptions = FindStreamSubscriptions(grainTypeOptions.Value.Classes, registry);
        }

        internal GrainId GetStreamSubscriptionMessageDeliverer(StreamSubscriptionMatch subscription)
        {
            return grainFactory.GetGrain(typeof(IStreamSubscriptionMessageDeliverer), subscription.MessageDelivererId).GetGrainId();
        }

        internal Guid GetSubscriptionId(StreamSubscriptionMatch subscription, QualifiedStreamId streamId)
        {
            var streamSubscriber = grainFactory.GetGrain(subscription.InterfaceType, subscription.Target).GetGrainId();
            return MakeSubscriptionId(streamSubscriber.Type, streamId);
        }

        internal Guid GetSubscriptionId(GrainId streamSubscriber, QualifiedStreamId streamId)
        {
            return MakeSubscriptionId(streamSubscriber.Type, streamId);
        }

        internal bool IsStreamSubscriptionSubscriber(GuidId subscriptionId, QualifiedStreamId streamId)
        {
            var possibleStreamSubscriptions = GetStreamSubscriptions(streamId);
            return HasImplicitSubscriptionMark(subscriptionId.Guid) && possibleStreamSubscriptions.Any();
        }

        internal StreamSubscriptionMatch GetStreamSubscription(QualifiedStreamId streamId, Guid subscriptionId)
        {
            foreach (var streamSubscription in GetStreamSubscriptions(streamId))
                if (subscriptionId == GetSubscriptionId(streamSubscription, streamId))
                    return streamSubscription;

            return null;
        }

        internal List<StreamSubscriptionMatch> GetStreamSubscriptions(QualifiedStreamId streamId)
        {
            if (!this.streamSubscriptions.TryGetValue(streamId.ProviderName, out var specifications))
                return new List<StreamSubscriptionMatch>(0);

            var resultCollection = new List<StreamSubscriptionMatch>();
            foreach (var specification in specifications)
            {
                if (!specification.IsMatch(streamId.StreamId, out var match))
                    continue;

                resultCollection.Add(match);
            }

            return resultCollection;
        }

        static Dictionary<string, List<StreamSubscriptionSpecification>> FindStreamSubscriptions(HashSet<Type> grainTypes, IDispatcherRegistry registry)
        {
            var specifications = new Dictionary<string, List<StreamSubscriptionSpecification>>();

            foreach (var grainType in grainTypes)
            {
                var streamSubscriptionAttributes = grainType.GetCustomAttributes<StreamSubscriptionAttribute>(true);
                var grainSpecifications = streamSubscriptionAttributes.Select(attr => StreamSubscriptionSpecificationBuilder.Build(grainType, attr, registry));

                foreach (var grainSpecification in grainSpecifications)
                {
                    if (!specifications.TryGetValue(grainSpecification.ProviderName, out var streams))
                    {
                        streams = new List<StreamSubscriptionSpecification>();
                        specifications[grainSpecification.ProviderName] = streams;
                    }

                    streams.Add(grainSpecification);
                }
            }

            return specifications;
        }

        /// <summary>
        ///     Create a subscriptionId that is unique per grainType, streamId, provider combination.
        /// </summary>
        static Guid MakeSubscriptionId(GrainType grainType, QualifiedStreamId streamId)
        {
            // adjusted code from orleans implicit stream subscription
            Span<byte> bytes = stackalloc byte[16];
            BinaryPrimitives.WriteUInt32LittleEndian(bytes, grainType.GetUniformHashCode());
            BinaryPrimitives.WriteUInt32LittleEndian(bytes[4..], (uint)streamId.StreamId.GetHashCode());
            BinaryPrimitives.WriteUInt32LittleEndian(bytes[8..], 0);
            BinaryPrimitives.WriteUInt32LittleEndian(bytes[12..], StableHash.ComputeHash(streamId.ProviderName));
            return MarkAsImplicitSubscription(new Guid(bytes));
        }

        static Guid MarkAsImplicitSubscription(Guid subscriptionId)
        {
            Span<byte> guidBytes = stackalloc byte[16];
            subscriptionId.TryWriteBytes(guidBytes);
            // set high bit of last byte
            guidBytes[15] |= 0x80;
            return new Guid(guidBytes);
        }

        static bool HasImplicitSubscriptionMark(Guid subscriptionId)
        {
            Span<byte> guidBytes = stackalloc byte[16];
            subscriptionId.TryWriteBytes(guidBytes);
            // return true if high bit of last byte is set
            return (guidBytes[15] & 0x80) != 0;
        }
    }
}