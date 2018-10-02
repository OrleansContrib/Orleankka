namespace Orleankka
{
    public interface ActorMessage<TActor> where TActor : IActorGrain
    {}

    public interface ActorMessage<TActor, TResult> where TActor : IActorGrain
    {}

    public static class ActorMessageExtensions
    {
        public static TResult Response<TActor, TResult>(this ActorMessage<TActor, TResult> message, TResult result) 
            where TActor : IActorGrain => result;
    }
}