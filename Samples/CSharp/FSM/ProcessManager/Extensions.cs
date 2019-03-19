using System;
using System.IO;
using System.Threading.Tasks;

using Orleankka.Services;

namespace Example
{
    public static class Extensions
    {
        public static void ScheduleOnce(this ITimerService timers, Func<Task> callback, TimeSpan? due = null) => 
            timers.Register(callback.Method.Name, due: due ?? TimeSpan.FromMilliseconds(1), callback);

        public static void DeleteFileIfExists(this string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}