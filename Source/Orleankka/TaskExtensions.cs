using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka
{
    static class TaskExtensions
    {
        public static async Task UnwrapExceptions(this Task task)
        {
            try
            {
                await task;
            }
            catch (AggregateException e)
            {
                throw e.OriginalExceptionPreservingStackTrace();
            }
        }

        public static async Task<T> UnwrapExceptions<T>(this Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (AggregateException e)
            {
                throw e.OriginalExceptionPreservingStackTrace();
            }
        }

        public static Exception OriginalExceptionPreservingStackTrace(this AggregateException e)
        {
            return PreserveStackTrace(OriginalException(e));
        }

        static Exception OriginalException(AggregateException e)
        {
            return e.Flatten().InnerExceptions.First();
        }

        static Exception PreserveStackTrace(Exception ex)
        {
            var remoteStackTraceString = typeof(Exception)
                .GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

            Debug.Assert(remoteStackTraceString != null);
            remoteStackTraceString.SetValue(ex, ex.StackTrace);

            return ex;
        }
    }
}
