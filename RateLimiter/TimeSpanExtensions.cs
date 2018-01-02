using System;

namespace RateLimiter
{
    internal static class TimeSpanExtensions
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, double times)
        {
            return TimeSpan.FromTicks((long)Math.Round(timeSpan.Ticks * times));
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, double divider)
        {
            return TimeSpan.FromTicks((long)Math.Round(timeSpan.Ticks / divider));
        }
    }
}
