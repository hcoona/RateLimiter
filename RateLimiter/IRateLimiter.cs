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

        bool TryAcquire();

#if !NET20
        Task<bool> TryAcquireAsync();

        Task<bool> TryAcquireAsync(CancellationToken cancellationToken);
#endif

        bool TryAcquire(int permits);

#if !NET20
        Task<bool> TryAcquireAsync(int permits);

        Task<bool> TryAcquireAsync(int permits, CancellationToken cancellationToken);
#endif

        bool TryAcquire(TimeSpan timeout);

#if !NET20
        Task<bool> TryAcquireAsync(TimeSpan timeout);

        Task<bool> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken);
#endif

        bool TryAcquire(int permits, TimeSpan timeout);

#if !NET20
        Task<bool> TryAcquireAsync(int permits, TimeSpan timeout);

        Task<bool> TryAcquireAsync(int permits, TimeSpan timeout, CancellationToken cancellationToken);
#endif

        TimeSpan Reserve(int permits);

#if !NET20
        Task<TimeSpan> ReserveAsync(int permits);

        Task<TimeSpan> ReserveAsync(int permits, CancellationToken cancellationToken);
#endif

        TimeSpan Query(int permits);

#if !NET20
        Task<TimeSpan> QueryAsync(int permits);

        Task<TimeSpan> QueryAsync(int permits, CancellationToken cancellationToken);
#endif
    }
}
