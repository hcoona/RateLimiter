using System;
using System.Threading;
using Xunit;

namespace RateLimiter.Tests
{
    public class RateLimiterTest
    {
        private readonly FakeStopwatchProviderAndBlocker stopwatchProviderAndBlocker =
            new FakeStopwatchProviderAndBlocker();

        internal IRateLimiter Create(double permitsPerSecond)
        {
            return Create(permitsPerSecond, 1.0);
        }

        internal IRateLimiter Create(double permitsPerSecond, double maxBurstSeconds)
        {
            return new SmoothBurstyRateLimiter(stopwatchProviderAndBlocker, maxBurstSeconds, stopwatchProviderAndBlocker)
            {
                PermitsPerSecond = permitsPerSecond
            };
        }

        internal IRateLimiter Create(double permitsPerSecond, TimeSpan warmupPeriod)
        {
            return new SmoothWarmingUpRateLimiter(stopwatchProviderAndBlocker, warmupPeriod, 3, stopwatchProviderAndBlocker)
            {
                PermitsPerSecond = permitsPerSecond
            };
        }

        [Fact]
        public void TestSimple()
        {
            var limiter = Create(5, 1);
            limiter.Acquire(); // R0.00, since it's the first request
            limiter.Acquire(); // R0.20
            limiter.Acquire(); // R0.20
            Assert.Equal(new[]
            {
                0L, TimeSpan.FromSeconds(0.2).Ticks, TimeSpan.FromSeconds(0.2).Ticks
            }, stopwatchProviderAndBlocker.Events);
        }

        [Fact]
        public void TestImmediateTryAcquire()
        {
            var limiter = Create(1);
            Assert.True(limiter.TryAcquire().Succeed, "Unable to acquire initial permit");
            Assert.False(limiter.TryAcquire().Succeed, "Capable of acquiring secondary permit");
        }

        [Fact]
        public void TestDoubleMinValueCanAcquireExactlyOnce()
        {
            var r = Create(double.Epsilon);
            Assert.True(r.TryAcquire().Succeed, "Unable to acquire initial permit");
            Assert.False(r.TryAcquire().Succeed, "Capable of acquiring an additional permit");
            stopwatchProviderAndBlocker.WaitAsync(TimeSpan.MaxValue.Subtract(TimeSpan.FromTicks(1)), CancellationToken.None).GetAwaiter().GetResult();
            Assert.False(r.TryAcquire().Succeed, "Capable of acquiring an additional permit after sleeping");
        }

        [Fact]
        public void TestSimpleRateUpdate()
        {
            var limiter = Create(5.0, TimeSpan.FromSeconds(5));
            Assert.Equal(5.0, limiter.PermitsPerSecond);
            limiter.PermitsPerSecond = 10.0;
            Assert.Equal(10.0, limiter.PermitsPerSecond);

            Assert.Throws<ArgumentOutOfRangeException>(() => limiter.PermitsPerSecond = 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => limiter.PermitsPerSecond = -10);
        }
    }
}
