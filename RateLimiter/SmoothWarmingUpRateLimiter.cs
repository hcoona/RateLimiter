using System;
using Clocks;

namespace RateLimiter
{
    internal sealed class SmoothWarmingUpRateLimiter : SmoothRateLimiter
    {
        public SmoothWarmingUpRateLimiter(
            IStopwatchProvider<long> stopwatchProvider,
            TimeSpan warmupPeriod,
            double coldFactor)
            : this(stopwatchProvider, warmupPeriod, coldFactor, null)
        {
        }

        internal SmoothWarmingUpRateLimiter(
            IStopwatchProvider<long> stopwatchProvider,
            TimeSpan warmupPeriod,
            double coldFactor,
            IAsyncBlocker asyncBlocker)
            : base(stopwatchProvider, asyncBlocker)
        {
            this.warmupPeriod = warmupPeriod;
            this.coldFactor = coldFactor;
        }

        private readonly TimeSpan warmupPeriod;

        private readonly double coldFactor;

        private double thresholdPermits;

        private TimeSpan slope;

        protected override TimeSpan CoolDownInterval => warmupPeriod.Divide(maxPermits);

        protected override void DoSetRate(double permitsPerSecond, TimeSpan stableInterval)
        {
            var oldMaxPermits = maxPermits;
            var coldInterval = stableInterval.Multiply(coldFactor);
            thresholdPermits = 0.5 * warmupPeriod.Ticks / stableInterval.Ticks;
            maxPermits = thresholdPermits + 2.0 * warmupPeriod.Ticks / (stableInterval + coldInterval).Ticks;
            slope = (coldInterval - stableInterval).Divide(maxPermits - thresholdPermits);
            if (oldMaxPermits == double.PositiveInfinity)
            {
                storedPermits = 0;
            }
            else
            {
                storedPermits = (oldMaxPermits == 0) ? maxPermits : (storedPermits * maxPermits / oldMaxPermits);
            }
        }

        protected override TimeSpan StoredPermitsToWaitTime(double storedPermits, double permitsToTake)
        {
            var availablePermitsAboveThreshold = storedPermits - thresholdPermits;
            var returnValue = TimeSpan.Zero;
            if (availablePermitsAboveThreshold > 0)
            {
                var permitsAboveThresholdToTake = Math.Min(availablePermitsAboveThreshold, permitsToTake);
                var length = PermitsToTime(availablePermitsAboveThreshold)
                    + PermitsToTime(availablePermitsAboveThreshold - permitsAboveThresholdToTake);
                returnValue = length.Multiply(permitsAboveThresholdToTake / 2.0);
                permitsToTake -= permitsAboveThresholdToTake;
            }
            returnValue += stableInterval.Multiply(permitsToTake);
            return returnValue;
        }

        private TimeSpan PermitsToTime(double permits)
        {
            return stableInterval + slope.Multiply(permits);
        }
    }
}
