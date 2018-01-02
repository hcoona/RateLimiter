using System;

namespace RateLimiter
{
    internal static class TimeSpanExtensions
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, double times)
        {
            var resultMillis = timeSpan.TotalMilliseconds * times;
            if (resultMillis < TimeSpan.MaxValue.TotalMilliseconds)
            {
                return TimeSpan.FromMilliseconds(resultMillis);
            }
            else
            {
                return TimeSpan.MaxValue;
            }
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, double divider)
        {
            var resultMillis = timeSpan.TotalMilliseconds / divider;
            if (resultMillis > TimeSpan.MinValue.TotalMilliseconds)
            {
                return TimeSpan.FromMilliseconds(resultMillis);
            }
            else
            {
                return TimeSpan.MinValue;
            }
        }
    }
}
