using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Meta;

    /// <summary>
    /// Base message interface for strongly typed actor messages
    /// </summary>
    /// <typeparam name="TActor">The type of the actor to which this message belongs</typeparam>
    public interface ActorMessage<TActor> : Message where TActor : IActorGrain, IGrainWithStringKey
    {}

    /// <summary>
    /// Base message interface for strongly typed actor messages which return results (ie queries)
    /// </summary>
    /// <typeparam name="TActor">The type of the actor to which this message belongs</typeparam>
    /// <typeparam name="TResult">The type of the returned result</typeparam>
    public interface ActorMessage<TActor, TResult> : ActorMessage<TActor>, Message<TResult> where TActor : IActorGrain, IGrainWithStringKey
    {}

    /// <summary>
    /// Extension methods for working with strongly typed actor messages
    /// </summary>
    public static class ActorMessageExtensions
    {
        [Obsolete("Please use Result method")]
        public static TResult Response<TActor, TResult>(this ActorMessage<TActor, TResult> message, TResult result) 
            where TActor : IActorGrain, IGrainWithStringKey => result;

        /// <summary>
        /// Helper method to use when building responses for strongly typed messages which return results.
        /// If the type of returned result is changed in message declaration this will help catching it at compile time.
        /// </summary>
        /// <example>
        /// <para>
        ///     class Query : ActorMessage&lt;MyActor,int&gt;{}
        /// </para>
        /// <para>
        ///     int On(Query x) => x.Result(42);
        /// </para>
        /// </example>
        /// <param name="message">The strongly typed message</param>
        /// <param name="result">The result to return</param>
        /// <typeparam name="TActor">The type of the actor to which this message belongs</typeparam>
        /// <typeparam name="TResult">The type of the returned result</typeparam>
        /// <returns>The value passed to <paramref name="result"/> argument</returns>
        public static TResult Result<TActor, TResult>(this ActorMessage<TActor, TResult> message, TResult result) 
            where TActor : IActorGrain, IGrainWithStringKey => result;

        /// <summary>
        /// Helper method to use when building task responses for strongly typed messages which return results.
        /// If the type of returned result is changed in message declaration this will help catching it at compile time.
        /// </summary>
        /// <example>
        /// <para>
        ///     class Query : ActorMessage&lt;MyActor,int&gt;{}
        /// </para>
        /// <para>
        ///     Task&lt;object&gt; On(Query x) => x.TaskResult(42);
        /// </para>
        /// </example>
        /// <param name="message">The strongly typed message</param>
        /// <param name="result">The result to return</param>
        /// <typeparam name="TActor">The type of the actor to which this message belongs</typeparam>
        /// <typeparam name="TResult">The type of the returned result</typeparam>
        /// <returns>The value passed to <paramref name="result"/> argument wrapped in Task&lt;object&gt;</returns>
        public static Task<object> TaskResult<TActor, TResult>(this ActorMessage<TActor, TResult> message, TResult result) 
            where TActor : IActorGrain, IGrainWithStringKey => Task.FromResult<object>(result);
    }
}