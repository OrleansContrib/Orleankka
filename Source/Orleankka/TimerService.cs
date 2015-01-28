using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    /// <summary>
    /// Manages registration of local actor timers
    /// </summary>
    public interface ITimerService
    {
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
    public class TimerService : ITimerService
    {
        readonly IDictionary<string, IDisposable> timers = new Dictionary<string, IDisposable>();
        readonly Func<IInternalTimerService> service;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerService"/> class.
        /// </summary>
        /// <param name="actor">The actor which requires timer services.</param>
        public TimerService(Actor actor) 
            : this(()=>actor.Host)
        {}

        TimerService(Func<IInternalTimerService> service)
        {
            this.service = service;
        }

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            timers.Add(id, service().RegisterTimer(s => callback(), null, due, period));
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            timers.Add(id, service().RegisterTimer(s => callback((TState)s), state, due, period));
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
