using System;
using System.Linq;

namespace Orleankka
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WorkerAttribute : Attribute
    {}
}
