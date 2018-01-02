using System;
using Clocks;

namespace RateLimiter
{
    public abstract class SmoothRateLimiter : RateLimiterBase
    {
        protected SmoothRateLimiter(IStopwatchProvider<long> stopwatchProvider) : base(stopwatchProvider)
        {
        }

        protected double storedPermits;

        protected double maxPermits;

        protected TimeSpan stableInterval;

        private long nextFreeTicketTimestamp = 0;

        protected abstract TimeSpan CoolDownInterval { get; }

        protected sealed override void DoSetRate(double permitsPerSecond, long nowTimestamp)
        {
            Resync(nowTimestamp);
            var stableInterval = TimeSpan.FromSeconds(1 / permitsPerSecond);
            this.stableInterval = stableInterval;
            DoSetRate(permitsPerSecond, stableInterval);
        }

        protected sealed override double DoGetRate()
        {
            return 1 / stableInterval.TotalSeconds;
        }

        protected sealed override long QueryEarliestAvailable(long nowTimestamp)
        {
            return nextFreeTicketTimestamp;
        }

        protected sealed override long ReserveEarliestAvailable(int requiredPermits, long nowTimestamp)
        {
            Resync(nowTimestamp);
            var returnValue = nextFreeTicketTimestamp;
            var storedPermitsToSpend = Math.Min(requiredPermits, storedPermits);
            var freshPermits = requiredPermits - storedPermitsToSpend;
            var waitTimeout = StoredPermitsToWaitTime(storedPermits, storedPermitsToSpend)
                + FreshPermitsToWaitTime(freshPermits);
            nextFreeTicketTimestamp = stopwatchProvider.GetNextTimestamp(nextFreeTicketTimestamp, waitTimeout);
            storedPermits -= storedPermitsToSpend;
            return returnValue;
        }

        protected abstract TimeSpan StoredPermitsToWaitTime(double storedPermits, double permitsToTake);

        private TimeSpan FreshPermitsToWaitTime(double permits)
        {
            return TimeSpan.FromTicks((long)Math.Round(stableInterval.Ticks * permits));
        }

        protected void Resync(long nowTimestamp)
        {
            if (nowTimestamp > nextFreeTicketTimestamp)
            {
                var newDuration = stopwatchProvider.ParseDuration(nextFreeTicketTimestamp, nowTimestamp);
                double newPermits = newDuration.Ticks / (double)CoolDownInterval.Ticks;
                storedPermits = Math.Min(maxPermits, storedPermits + newPermits);
                nextFreeTicketTimestamp = nowTimestamp;
            }
        }

        protected abstract void DoSetRate(double permitsPerSecond, TimeSpan stableInterval);
    }
}
