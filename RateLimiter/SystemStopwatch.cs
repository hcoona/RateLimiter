using System;
using System.Diagnostics;
using Clocks;

namespace RateLimiter
{
    public class SystemStopwatch : IStopwatch
    {
        Stopwatch _sw;
        public SystemStopwatch()
        {
            _sw = new Stopwatch();
        }

        public TimeSpan Elapsed => _sw.Elapsed;

        public bool IsRunning => _sw.IsRunning;

        public void Reset() => _sw.Reset();

        public void Restart() => _sw.Restart();

        public void Start() => _sw.Start();

        public void Stop() => _sw.Start();
    }
}
