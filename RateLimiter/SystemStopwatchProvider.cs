using System;
using System.Collections.Generic;
using System.Text;
using Clocks;

namespace RateLimiter
{
    public class SystemStopwatchProvider : IStopwatchProvider<long>
    {
        public bool IsHighResolution => false;

        public IStopwatch Create() => new SystemStopwatch();

        public long GetNextTimestamp(long from, TimeSpan interval) => interval.Ticks + from;

        public long GetTimestamp() => DateTime.Now.Ticks;

        public TimeSpan ParseDuration(long from, long to) => TimeSpan.FromTicks(to - from);

        public IStopwatch StartNew()
        {
            var sw = new SystemStopwatch();
            sw.Start();
            return sw;
        }
    }
}
