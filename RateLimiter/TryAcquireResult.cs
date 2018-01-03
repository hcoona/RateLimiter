using System;

namespace RateLimiter
{
    public class TryAcquireResult
    {
        public bool Succeed { get; set; }

        public TimeSpan MomentAvailableInterval { get; set; }
    }
}
