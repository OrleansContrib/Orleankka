using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka.Services
{
    using Core;

    /// <summary>
    /// Manages registration of local actor timers
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        ///     Registers a one-off timer to a due time to fire <see cref="Timer"/> message to this actor in a reentrant way.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     The timer will fire off ony once.
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's will be logged
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for firing a timer tick.</param>
        /// <param name="state">State object that will be passed with Timer message.</param>
        void Register(string id, TimeSpan due, object state = null);

        /// <summary>
        ///     Registers a timer to send periodic <see cref="Timer"/> message to this actor in a reentrant way.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     Until the Task returned from the actor's receiver is resolved,
        ///     the next timer tick will not be scheduled.
        ///     That is to say, timer callbacks never interleave their turns.
        /// </para>
        /// <para>
        ///     The timer may be stopped at any time by calling the <see cref="Unregister(string)"/> method
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's 
        ///     will be logged, but will not prevent the next timer tick from being queued.
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="state">State object that will be passed with Timer message.</param>>
        void Register(string id, TimeSpan due, TimeSpan period, object state = null);

        /// <summary>
        ///     Registers a one-off timer to a due time to fire <see cref="Timer"/> message to this actor in a reentrant way.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     The timer will fire off ony once.
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's will be logged
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for firing timer tick.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void Register(string id, TimeSpan due, Func<Task> callback);

        /// <summary>
        ///     Registers a timer to send periodic callbacks to this actor in a reentrant way.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     Until the Task returned from the <paramref name="callback"/> is resolved,
        ///     the next timer tick will not be scheduled.
        ///     That is to say, timer callbacks never interleave their turns.
        /// </para>
        /// <para>
        ///     The timer may be stopped at any time by calling the <see cref="Unregister(string)"/> method
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's returned from the  <paramref name="callback"/>
        ///     will be logged, but will not prevent the next timer tick from being queued.
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback);

        /// <summary>
        ///     Registers a one-off timer to a due time to fire <see cref="Timer"/> message to this actor in a reentrant way.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     The timer will fire off ony once.
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's will be logged
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for firing timer tick.</param>
        /// <param name="state">State object that will be passed as argument when calling the <paramref name="callback"/>.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void Register<TState>(string id, TimeSpan due, TState state, Func<TState, Task> callback);

        /// <summary>
        ///     Registers a timer to send periodic callbacks to this actor in a reentrant way.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     Until the Task returned from the <paramref name="callback"/> is resolved,
        ///     the next timer tick will not be scheduled.
        ///     That is to say, timer callbacks never interleave their turns.
        /// </para>
        /// <para>
        ///     The timer may be stopped at any time by calling the <see cref="Unregister(string)"/> method
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's returned from the  <paramref name="callback"/>
        ///     will be logged, but will not prevent the next timer tick from being queued.
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the <paramref name="callback"/>.</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        void Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback);

        /// <summary>
        /// Unregister previously registered timer. 
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        void Unregister(string id);

        /// <summary>
        /// Checks whether timer with the given name was registered before
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        /// <returns><c>true</c> if timer was the give name was previously registered, <c>false</c> otherwise </returns>
        bool IsRegistered(string id);

        /// <summary>
        /// Returns ids of all currently registered timers
        /// </summary>
        /// <returns>Sequence of <see cref="string"/> elements</returns>
        IEnumerable<string> Registered();
    }

    /// <summary>
    /// Default Orleans bound implementation of <see cref="ITimerService"/>
    /// </summary>
    class TimerService : ITimerService
    {
        readonly IDictionary<string, IDisposable> timers = new Dictionary<string, IDisposable>();
        readonly ActorEndpoint endpoint;

        internal TimerService(ActorEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        void ITimerService.Register(string id, TimeSpan due, object state)
        {
            timers.Add(id, endpoint.RegisterTimer(s =>
            {
                ((ITimerService) this).Unregister(id);
                return endpoint.ReceiveInternal(new Timer(id, s));
            },
            state, due, TimeSpan.FromMilliseconds(1)));
        }

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, object state)
        {
            timers.Add(id, endpoint.RegisterTimer(s => endpoint.ReceiveInternal(new Timer(id, s)), state, due, period));
        }

        void ITimerService.Register(string id, TimeSpan due, Func<Task> callback)
        {
            ((ITimerService) this).Register(id, due, TimeSpan.FromMilliseconds(1), () =>
            {
                ((ITimerService) this).Unregister(id);
                return callback();
            });
        }

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            timers.Add(id, endpoint.RegisterTimer(s => callback(), null, due, period));
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TState state, Func<TState, Task> callback)
        {
            ((ITimerService) this).Register(id, due, TimeSpan.FromMilliseconds(1), state, s =>
            {
                ((ITimerService) this).Unregister(id);
                return callback(s);
            });
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            timers.Add(id, endpoint.RegisterTimer(s => callback((TState) s), state, due, period));
        }

        void ITimerService.Unregister(string id)
        {
            var timer = timers[id];
            timers.Remove(id);
            timer.Dispose();
        }

        bool ITimerService.IsRegistered(string id)
        {
            return timers.ContainsKey(id);
        }

        IEnumerable<string> ITimerService.Registered()
        {
            return timers.Keys;
        }
    }
}
