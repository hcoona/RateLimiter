using System;
#if !NET20
using System.Threading;
using System.Threading.Tasks;
#endif

namespace RateLimiter
{
    public interface IRateLimiter
    {
        double PermitsPerSecond { get; set; }

        TimeSpan Acquire();

        TimeSpan Acquire(int permits);

#if !NET20
        Task<TimeSpan> AcquireAsync();

        Task<TimeSpan> AcquireAsync(CancellationToken cancellationToken);

        Task<TimeSpan> AcquireAsync(int permits);

        Task<TimeSpan> AcquireAsync(int permits, CancellationToken cancellationToken);
#endif

        TryAcquireResult TryAcquire();

#if !NET20
        Task<TryAcquireResult> TryAcquireAsync();

        Task<TryAcquireResult> TryAcquireAsync(CancellationToken cancellationToken);
#endif

        TryAcquireResult TryAcquire(int permits);

#if !NET20
        Task<TryAcquireResult> TryAcquireAsync(int permits);

        Task<TryAcquireResult> TryAcquireAsync(int permits, CancellationToken cancellationToken);
#endif

        TryAcquireResult TryAcquire(TimeSpan timeout);

#if !NET20
        Task<TryAcquireResult> TryAcquireAsync(TimeSpan timeout);

        Task<TryAcquireResult> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken);
#endif

        TryAcquireResult TryAcquire(int permits, TimeSpan timeout);

#if !NET20
        Task<TryAcquireResult> TryAcquireAsync(int permits, TimeSpan timeout);

        Task<TryAcquireResult> TryAcquireAsync(int permits, TimeSpan timeout, CancellationToken cancellationToken);
#endif

        TimeSpan Reserve(int permits);

#if !NET20
        Task<TimeSpan> ReserveAsync(int permits);

        Task<TimeSpan> ReserveAsync(int permits, CancellationToken cancellationToken);
#endif
    }
}
