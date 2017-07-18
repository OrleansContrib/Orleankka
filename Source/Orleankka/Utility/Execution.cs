using System;
using System.Diagnostics;

namespace Orleankka.Utility
{
    class Execution
    {
        public static IDisposable Trace(string label) => new Session(Stopwatch.StartNew(), label);

        class Session : IDisposable
        {
            readonly Stopwatch stopwatch;
            readonly string label;

            public Session(Stopwatch stopwatch, string label)
            {
                this.stopwatch = stopwatch;
                this.label = label;
            }

            public void Dispose() => System.Diagnostics.Trace.TraceInformation($"{label} done in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}