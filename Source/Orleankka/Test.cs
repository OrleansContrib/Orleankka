using Orleankka;
using Orleankka.Core;
using Orleankka.Core.Endpoints;

public interface IOrleankka.Features.Using_reminders.TestActorEndpoint : IActorEndpoint {}
public class Orleankka.Features.Using_reminders.TestActorEndpoint : ActorEndpoint, IOrleankka.Features.Using_reminders.TestActorEndpoint { }
public interface IOrleankka.Features.Unwrapping_exceptions.TestActorEndpoint : IActorEndpoint {}
public class Orleankka.Features.Unwrapping_exceptions.TestActorEndpoint : ActorEndpoint, IOrleankka.Features.Unwrapping_exceptions.TestActorEndpoint { }
public interface IOrleankka.Features.Unwrapping_exceptions.TestInsideActorEndpoint : IActorEndpoint {}
public class Orleankka.Features.Unwrapping_exceptions.TestInsideActorEndpoint : ActorEndpoint, IOrleankka.Features.Unwrapping_exceptions.TestInsideActorEndpoint { }

public interface IT2Endpoint : IActorEndpoint { }
public class T2Endpoint : ActorEndpoint, IT2Endpoint { }
public interface IT1Endpoint : IActorEndpoint { }
public class T1Endpoint : ActorEndpoint, IT1Endpoint { }

public interface IOrleankka.Features.Request_response.TestActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Request_response.TestActorEndpoint : ActorEndpoint, IOrleankka.Features.Request_response.TestActorEndpoint { }
public interface IOrleankka.Features.Request_response.TestInsideActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Request_response.TestInsideActorEndpoint : ActorEndpoint, IOrleankka.Features.Request_response.TestInsideActorEndpoint { }
public interface IOrleankka.Features.Observing_notifications.TestActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Observing_notifications.TestActorEndpoint : ActorEndpoint, IOrleankka.Features.Observing_notifications.TestActorEndpoint { }
public interface IOrleankka.Features.Observing_notifications.TestInsideActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Observing_notifications.TestInsideActorEndpoint : ActorEndpoint, IOrleankka.Features.Observing_notifications.TestInsideActorEndpoint { }
public interface IOrleankka.Features.Reentrant_messages.TestActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Reentrant_messages.TestActorEndpoint : ActorEndpoint, IOrleankka.Features.Reentrant_messages.TestActorEndpoint { }
public interface IOrleankka.Features.Reentrant_messages.TestActor2Endpoint : IActorEndpoint {}
                   public class Orleankka.Features.Reentrant_messages.TestActor2Endpoint : ActorEndpoint, IOrleankka.Features.Reentrant_messages.TestActor2Endpoint { }
public interface IOrleankka.Features.Stream_references.AzureQueueStreamProviderVerification.TestProducerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Stream_references.AzureQueueStreamProviderVerification.TestProducerActorEndpoint : ActorEndpoint, IOrleankka.Features.Stream_references.AzureQueueStreamProviderVerification.TestProducerActorEndpoint { }
public interface IOrleankka.Features.Stream_references.AzureQueueStreamProviderVerification.TestConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Stream_references.AzureQueueStreamProviderVerification.TestConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Stream_references.AzureQueueStreamProviderVerification.TestConsumerActorEndpoint { }
public interface IOrleankka.Features.Stream_references.SimpleMessageStreamProviderVerification.TestProducerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Stream_references.SimpleMessageStreamProviderVerification.TestProducerActorEndpoint : ActorEndpoint, IOrleankka.Features.Stream_references.SimpleMessageStreamProviderVerification.TestProducerActorEndpoint { }
public interface IOrleankka.Features.Stream_references.SimpleMessageStreamProviderVerification.TestConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Stream_references.SimpleMessageStreamProviderVerification.TestConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Stream_references.SimpleMessageStreamProviderVerification.TestConsumerActorEndpoint { }
public interface IOrleankka.Features.Stream_subscriptions.AzureQueueStreamProviderVerification.TestConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Stream_subscriptions.AzureQueueStreamProviderVerification.TestConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Stream_subscriptions.AzureQueueStreamProviderVerification.TestConsumerActorEndpoint { }
public interface IOrleankka.Features.Stream_subscriptions.SimpleMessageStreamProviderVerification.TestConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Stream_subscriptions.SimpleMessageStreamProviderVerification.TestConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Stream_subscriptions.SimpleMessageStreamProviderVerification.TestConsumerActorEndpoint { }
public interface IOrleankka.Features.Keep_alive.TestActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Keep_alive.TestActorEndpoint : ActorEndpoint, IOrleankka.Features.Keep_alive.TestActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.TestProducerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.TestProducerActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.TestProducerActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestClientToStreamConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestClientToStreamConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestClientToStreamConsumerActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestActorToStreamConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestActorToStreamConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestActorToStreamConsumerActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestMultistreamSubscriptionWithFixedIdsActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestMultistreamSubscriptionWithFixedIdsActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestMultistreamSubscriptionWithFixedIdsActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestMultistreamRegexBasedSubscriptionActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestMultistreamRegexBasedSubscriptionActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestMultistreamRegexBasedSubscriptionActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestDeclaredHandlerOnlyAutomaticFilterActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestDeclaredHandlerOnlyAutomaticFilterActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestDeclaredHandlerOnlyAutomaticFilterActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestSelectAllFilterActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestSelectAllFilterActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestSelectAllFilterActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestExplicitFilterActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestExplicitFilterActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestExplicitFilterActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestDynamicTargetSelectorActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestDynamicTargetSelectorActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.AzureQueueStreamProviderVerification.TestDynamicTargetSelectorActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestClientToStreamConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestClientToStreamConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestClientToStreamConsumerActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestActorToStreamConsumerActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestActorToStreamConsumerActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestActorToStreamConsumerActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestMultistreamSubscriptionWithFixedIdsActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestMultistreamSubscriptionWithFixedIdsActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestMultistreamSubscriptionWithFixedIdsActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestMultistreamRegexBasedSubscriptionActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestMultistreamRegexBasedSubscriptionActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestMultistreamRegexBasedSubscriptionActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestDeclaredHandlerOnlyAutomaticFilterActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestDeclaredHandlerOnlyAutomaticFilterActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestDeclaredHandlerOnlyAutomaticFilterActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestSelectAllFilterActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestSelectAllFilterActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestSelectAllFilterActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestExplicitFilterActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestExplicitFilterActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestExplicitFilterActorEndpoint { }
public interface IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestDynamicTargetSelectorActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestDynamicTargetSelectorActorEndpoint : ActorEndpoint, IOrleankka.Features.Declarative_stream_subscriptions.SimpleMessageStreamProviderVerification.TestDynamicTargetSelectorActorEndpoint { }
public interface IOrleankka.Features.Handler_wiring.Tests+TestActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Features.Handler_wiring.Tests+TestActorEndpoint : ActorEndpoint, IOrleankka.Features.Handler_wiring.Tests+TestActorEndpoint { }
public interface IOrleankka.Checks.ObserverCollectionFixture+TestObservableActorEndpoint : IActorEndpoint {}
                   public class Orleankka.Checks.ObserverCollectionFixture+TestObservableActorEndpoint : ActorEndpoint, IOrleankka.Checks.ObserverCollectionFixture+TestObservableActorEndpoint { }
