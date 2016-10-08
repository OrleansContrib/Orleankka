using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Core
{
    /// <summary>
    /// A utility class that provides serial execution of async functions.
    /// In can be used inside reentrant grain code to execute some methods in a non-reentrant (serial) way.
    /// Source: https://github.com/dotnet/orleans/blob/master/src/Orleans/Async/AsyncSerialExecutor.cs
    /// </summary>
    class AsyncSerialExecutor
    {
        readonly ConcurrentQueue<Tuple<TaskCompletionSource<object>, Func<Task<object>>>> actions = 
            new ConcurrentQueue<Tuple<TaskCompletionSource<object>, Func<Task<object>>>>();

        readonly InterlockedExchangeLock locker = new InterlockedExchangeLock();

        class InterlockedExchangeLock
        {
            const int Locked = 1;
            const int Unlocked = 0;
            int lockState = Unlocked;

            public bool TryGetLock()  => Interlocked.Exchange(ref lockState, Locked) != Locked;
            public void ReleaseLock() => Interlocked.Exchange(ref lockState, Unlocked);
        }

        /// <summary>
        /// Submit the next function for execution. It will execute after all previously submitted functions have finished, without interleaving their executions.
        /// Returns a promise that represents the execution of this given function. 
        /// The returned promise will be resolved when the given function is done executing.
        /// </summary>
        public Task<object> AddNext(Func<Task<object>> func)
        {
            var resolver = new TaskCompletionSource<object>();
            actions.Enqueue(new Tuple<TaskCompletionSource<object>, Func<Task<object>>>(resolver, func));
            var task = resolver.Task;
            ExecuteNext().Ignore();
            return task;
        }

        async Task ExecuteNext()
        {
            while (!actions.IsEmpty)
            {
                var gotLock = false;

                try
                {
                    if (!(gotLock = locker.TryGetLock()))
                        return;

                    while (!actions.IsEmpty)
                    {
                        Tuple<TaskCompletionSource<object>, Func<Task<object>>> actionTuple;
                        if (actions.TryDequeue(out actionTuple))
                        {
                            try
                            {
                                var result = await actionTuple.Item2();
                                actionTuple.Item1.TrySetResult(result);
                            }
                            catch (Exception exc)
                            {
                                actionTuple.Item1.TrySetException(exc);
                            }
                        }
                    }
                }
                finally
                {
                    if (gotLock)
                        locker.ReleaseLock();
                }
            }
        }
    }
}