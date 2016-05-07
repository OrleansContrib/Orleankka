namespace Orleankka
{
    using Core;
    using Core.Endpoints;

    static class ActorPathExtensions
    {
        internal static ActorType Type(this ActorPath path) => ActorType.Registered(path.Code);
        internal static ActorInterface Interface(this ActorPath path) => path.Type().Interface;
        internal static ActorImplementation Implementation(this ActorPath path) => path.Type().Implementation;
        internal static IActorEndpoint Proxy(this ActorPath path) => path.Type().Interface.Proxy(path);
    }
}