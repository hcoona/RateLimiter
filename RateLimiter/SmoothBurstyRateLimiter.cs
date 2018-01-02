using System;
using Clocks;

namespace RateLimiter
{
    internal sealed class SmoothBurstyRateLimiter : SmoothRateLimiter
    {
        public SmoothBurstyRateLimiter(
            IStopwatchProvider<long> stopwatchProvider,
            double maxBurstSeconds)
#if !NET20
            : this(stopwatchProvider, maxBurstSeconds, null)
        {
        }

        internal SmoothBurstyRateLimiter(
            IStopwatchProvider<long> stopwatchProvider,
            double maxBurstSeconds,
            IAsyncBlocker asyncBlocker)
            : base(stopwatchProvider, asyncBlocker)
#else
            : base(stopwatchProvider)
#endif
        {
            this.maxBurstSeconds = maxBurstSeconds;
        }

        private readonly double maxBurstSeconds;

        protected override TimeSpan CoolDownInterval => stableInterval;

        protected override void DoSetRate(double permitsPerSecond, TimeSpan stableInterval)
        {
            var oldMaxPermits = maxPermits;
            maxPermits = maxBurstSeconds * permitsPerSecond;
            if (double.IsPositiveInfinity(oldMaxPermits))
            {
                storedPermits = maxPermits;
            }
            else
            {
                storedPermits = (oldMaxPermits == 0) ? 0 : storedPermits * maxPermits / oldMaxPermits;
            }
        }

        protected override TimeSpan StoredPermitsToWaitTime(double storedPermits, double permitsToTake)
        {
            return TimeSpan.Zero;
        }
    }
}
