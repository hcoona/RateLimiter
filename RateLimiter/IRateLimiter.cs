using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public interface IRateLimiter
    {
        double PermitsPerSecond { get; set; }

        TimeSpan Acquire();

        TimeSpan Acquire(int permits);

        Task<TimeSpan> AcquireAsync();

        Task<TimeSpan> AcquireAsync(CancellationToken cancellationToken);

        Task<TimeSpan> AcquireAsync(int permits);

        Task<TimeSpan> AcquireAsync(int permits, CancellationToken cancellationToken);

        bool TryAcquire();

        Task<bool> TryAcquireAsync();

        Task<bool> TryAcquireAsync(CancellationToken cancellationToken);

        bool TryAcquire(int permits);

        Task<bool> TryAcquireAsync(int permits);

        Task<bool> TryAcquireAsync(int permits, CancellationToken cancellationToken);

        bool TryAcquire(TimeSpan timeout);

        Task<bool> TryAcquireAsync(TimeSpan timeout);

        Task<bool> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken);

        bool TryAcquire(int permits, TimeSpan timeout);

        Task<bool> TryAcquireAsync(int permits, TimeSpan timeout);

        Task<bool> TryAcquireAsync(int permits, TimeSpan timeout, CancellationToken cancellationToken);

        TimeSpan Reserve(int permits);

        Task<TimeSpan> ReserveAsync(int permits);

        Task<TimeSpan> ReserveAsync(int permits, CancellationToken cancellationToken);

        TimeSpan Query(int permits);

        Task<TimeSpan> QueryAsync(int permits);

        Task<TimeSpan> QueryAsync(int permits, CancellationToken cancellationToken);
    }
}
