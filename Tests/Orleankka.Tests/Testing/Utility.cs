using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orleankka.Testing
{
    static class Try
    {
        public static async Task Until(Func<Task<bool>> action, TimeSpan? due = null, TimeSpan? timeout = null)
        {
            var wait = timeout ?? TimeSpan.FromSeconds(2);
                
            if (due != null)
                await Task.Delay(due.Value);

            SpinWait.SpinUntil(() => action().GetAwaiter().GetResult(), wait);
        }
    }
}