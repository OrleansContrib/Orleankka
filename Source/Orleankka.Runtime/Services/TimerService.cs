using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;
using Orleans.Timers;

namespace Orleankka.Services
{
    using Orleans.Runtime;

    /// <summary>
    /// Manages registration of local actor timers
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        ///     Registers a one-off timer to a due time to execute a given callback.
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
        ///     Registers a timer to periodically execute a given callback.
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
        ///     Registers a one-off timer to a due time to execute a given callback.
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
        ///     Registers a timer to periodically execute a given callback.
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
        ///     Registers a one-off timer to a due time to send <see cref="Timer"/> message
        ///     to this actor <see cref="ActorGrain.Receive"/> function.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     Until the Task returned from the <see cref="ActorGrain.Receive"/> is resolved,
        ///     the next timer tick will not be scheduled.
        ///     That is to say, timer callbacks never interleave their turns.
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's will be logged
        ///     unless the <paramref name="ignore"/> is set to <c>true</c>
        /// </para>
        /// <para>
        ///     For non-interleaved delivery set <paramref name="interleave"/>
        ///     to <c>false</c>. The message will be delivered via
        ///     <see cref="ActorGrain.Self"/> method. If used in conjunction
        ///     with <paramref name="ignore"/> set to <c>false</c> the message
        ///     will be delivered as OneWay for most optimal performance
        /// </para>
        /// <para>
        ///     Use <paramref name="message"/> parameter to send custom message
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for sending timer message.</param>
        /// <param name="interleave">Controls timer message interleaving. Set to <c>false</c> for non-interleaved delivery</param>
        /// <param name="ignore">If set to <c>true</c> the message will be sent in fire-and-forget fashion</param>
        /// <param name="message">Custom message to send on timer tick</param>
        void Register(string id, TimeSpan due, bool interleave = true, bool ignore = false, object message = null);

        /// <summary>
        ///     Registers a timer to periodically send <see cref="Timer"/> message
        ///     to a given actor <see cref="ActorGrain.Receive"/> function.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This timer will not prevent the current grain from being deactivated.
        ///     If the grain is deactivated, then the timer will be discarded.
        /// </para>
        /// <para>
        ///     Until the Task returned from the <see cref="ActorGrain.Receive"/> is resolved, the next timer tick
        ///     will not be scheduled. That is to say, timer callbacks never interleave their own turns.
        /// </para>
        ///     <b>NOTE:</b> If <paramref name="fireAndForget"/> is set to <c>true</c> the next timer tick will be scheduled immediately.
        /// <para>
        /// </para>
        /// <para>
        ///     The timer may be stopped at any time by calling the
        ///    <see cref="ITimerService.Unregister(string)"/> method
        /// </para>
        /// <para>
        ///     Any exceptions thrown by or faulted Task's returned from the <see cref="ActorGrain.Receive"/>
        ///     will be logged (unless the <paramref name="fireAndForget"/> is set to <c>true</c>),
        ///     but will not prevent the next timer tick from being queued.
        /// </para>
        /// <para>
        ///     For non-interleaved delivery set <paramref name="interleave"/>
        ///     to <c>false</c>. The message will be delivered via
        ///     <see cref="ActorGrain.Self"/> method. If used in conjunction
        ///     with <paramref name="fireAndForget"/> set to <c>false</c> the message
        ///     will be delivered as OneWay for most optimal performance
        /// </para>
        /// <para>
        ///     Use <paramref name="message"/> parameter to send custom message
        /// </para>
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="due">Due time for sending timer message.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <param name="interleave">Controls timer message interleaving. Set to <c>false</c> for non-interleaved delivery</param>
        /// <param name="fireAndForget">If set to <c>true</c> the message will be sent in fire-and-forget fashion</param>
        /// <param name="message">Custom message to send on timer tick</param>
        void Register(string id, TimeSpan due, TimeSpan period, bool interleave = true, bool fireAndForget = false, object message = null);

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
        readonly ITimerRegistry registry;
        readonly ActorGrain grain;

        internal TimerService(ActorGrain grain)
        {
            this.grain = grain;
            this.registry = grain.Runtime().TimerRegistry;
        }

        IGrainContext GrainContext() => ((IGrainBase)grain).GrainContext;

        void ITimerService.Register(string id, TimeSpan due, Func<Task> callback)
        {
            ((ITimerService) this).Register(id, due, TimeSpan.FromMilliseconds(1), async () =>
            {
                ((ITimerService) this).Unregister(id);
                await callback();
            });
        }

        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, Func<Task> callback)
        {
            timers.Add(id, registry.RegisterTimer(GrainContext(), async s => await callback(), null, due, period));
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TState state, Func<TState, Task> callback)
        {
            ((ITimerService) this).Register(id, due, TimeSpan.FromMilliseconds(1), state, async s =>
            {
                ((ITimerService) this).Unregister(id);
                await callback(s);
            });
        }

        void ITimerService.Register<TState>(string id, TimeSpan due, TimeSpan period, TState state, Func<TState, Task> callback)
        {
            timers.Add(id, registry.RegisterTimer(GrainContext(), async s => await callback((TState) s), state, due, period));
        }

        void ITimerService.Register(string id, TimeSpan due, bool interleave, bool fireAndForget, object message)
        {
            var msg = message ?? new Timer(id);

            ((ITimerService) this).Register(id, due, TimeSpan.FromMilliseconds(15), async () =>
            {
                ((ITimerService) this).Unregister(id);

                await SendTimerMessage(id, interleave, fireAndForget, msg);
            });
        }
        
        void ITimerService.Register(string id, TimeSpan due, TimeSpan period, bool interleave, bool fireAndForget, object message)
        {
            var msg = message ?? new Timer(id);

            ((ITimerService) this).Register(id, due, period, async () =>
            {
                await SendTimerMessage(id, interleave, fireAndForget, msg);
            });
        }

        async Task SendTimerMessage(string id, bool interleave, bool fireAndForget, object message)
        {
            if (interleave)
            {
                var task = grain.ReceiveRequest(message);
                if (fireAndForget)
                {
                    task.Ignore();
                    return;
                }

                await task;
                return;
            }

            if (fireAndForget)
            {
                grain.Self.Notify(message);
                return;
            }

            await grain.Self.Tell(message);
        }

        void ITimerService.Unregister(string id)
        {
            if (!timers.TryGetValue(id, out var timer))
                return;

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