using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Clocks;

namespace RateLimiter.Tests
{
    internal class FakeStopwatchProviderAndBlocker : IStopwatchProvider<long>, IAsyncBlocker
    {
        private long instant = 0;

        internal IList<long> Events { get; } = new List<long>();

        public long GetTimestamp()
        {
            return instant;
        }

        public TimeSpan ParseDuration(long from, long to)
        {
            return TimeSpan.FromTicks(to - from);
        }

        public long GetNextTimestamp(long from, TimeSpan interval)
        {
            return from + interval.Ticks;
        }

        public Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            instant += timeout.Ticks;
            Events.Add(timeout.Ticks);
            return Task.FromResult<object>(null);
        }

        public bool IsHighResolution => throw new NotImplementedException();

        public IStopwatch Create()
        {
            throw new NotImplementedException();
        }

        public IStopwatch StartNew()
        {
            throw new NotImplementedException();
        }
    }
}
