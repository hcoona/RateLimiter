using System;
using Clocks;

namespace RateLimiter
{
    public static class RateLimiter
    {
        public static IRateLimiter CreateBursty(
            double permitsPerSecond,
            double maxBurstSeconds,
            IStopwatchProvider<long> stopwatchProvider)
        {
            return new SmoothBurstyRateLimiter(stopwatchProvider, maxBurstSeconds)
            {
                PermitsPerSecond = permitsPerSecond
            };
        }

        public static IRateLimiter CreateWarmingUp(
            double permitsPerSecond,
            TimeSpan warmupPeriod,
            double coldFactor,
            IStopwatchProvider<long> stopwatchProvider)
        {
            return new SmoothWarmingUpRateLimiter(stopwatchProvider, warmupPeriod, coldFactor)
            {
                PermitsPerSecond = permitsPerSecond
            };
        }

        public static IRateLimiter Create(
            double permitsPerSecond,
            IStopwatchProvider<long> stopwatchProvider)
        {
            return CreateBursty(permitsPerSecond, 1, stopwatchProvider);
        }

        public static IRateLimiter Create(
            double permitsPerSecond,
            TimeSpan warmupPeriod,
            IStopwatchProvider<long> stopwatchProvider)
        {
            return CreateWarmingUp(permitsPerSecond, warmupPeriod, 3, stopwatchProvider);
        }
    }
}
