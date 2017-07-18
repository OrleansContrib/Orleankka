using System;
using System.Diagnostics;

namespace Orleankka.Utility
{
    public class Trace
    {
        static readonly TraceSource Source = new TraceSource("Orleankka", SourceLevels.All);

        public static IDisposable Execution(string label) => new Session(Stopwatch.StartNew(), label);

        class Session : IDisposable
        {
            readonly Stopwatch stopwatch;
            readonly string label;

            public Session(Stopwatch stopwatch, string label)
            {
                this.stopwatch = stopwatch;
                this.label = label;
            }

            public void Dispose() => Source.TraceInformation($"{label} done in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}