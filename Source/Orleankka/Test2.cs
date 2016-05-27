using Orleankka;
using Orleankka.Core;
using Orleankka.Core.Endpoints;

namespace Fun.Orleankka.Features.Using_reminders
{
    public interface ITestActor : global::Orleankka.Core.Endpoints.IActorEndpoint { }
    public class TestActor : global::Orleankka.Core.ActorEndpoint, ITestActor { }
}