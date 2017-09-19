using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka.Utility
{
    public class TaskResult
    {
        public static readonly Task<object> Done = Task.FromResult((object)null);
    }
}