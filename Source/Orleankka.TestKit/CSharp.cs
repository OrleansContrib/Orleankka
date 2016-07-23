using Orleankka.CSharp;

namespace Orleankka.TestKit
{
    namespace CSharp
    {
        public static class ActorRefMockExtensions
        {
            public static ActorRefMock MockActorOf<TActor>(this ActorSystemMock system, string id) where TActor : IActor
            {
                var path = typeof(TActor).ToActorPath(id);
                return system.MockActorOf(path);
            }
        }
    }
}