using System.Diagnostics;
using System.Reflection;

using Orleans.Concurrency;

namespace Orleans.Internals
{
    public static class StatelessWorkerUtility
    {
        public static int MaxLocalWorkers(this StatelessWorkerAttribute worker)
        {
            var att = typeof(StatelessWorkerAttribute).Assembly.GetType("Orleans.Runtime.StatelessWorkerPlacement");
            var prop = att.GetProperty("MaxLocal", BindingFlags.Instance | BindingFlags.Public);
            Debug.Assert(prop != null);
            return (int) prop.GetValue(worker.PlacementStrategy, new object[0]);
        }
    }
}