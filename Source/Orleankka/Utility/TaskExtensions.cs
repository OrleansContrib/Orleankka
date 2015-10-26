using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Orleankka.Utility
{
    static class TaskExtensions
    {
        public static async Task<T> UnwrapExceptions<T>(this Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (AggregateException e)
            {
                ExceptionDispatchInfo.Capture(e.GetBaseException()).Throw();
            }

            throw new Exception("unreachable");
        }

        public static async Task UnwrapExceptions(this Task task)
        {
            try
            {
                await task;
            }
            catch (AggregateException e)
            {
                ExceptionDispatchInfo.Capture(e.GetBaseException()).Throw();
            }
        }
    }
}
