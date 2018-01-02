using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Clocks;

namespace RateLimiter
{
    public abstract class RateLimiterBase : IRateLimiter
    {
        protected readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        protected readonly IStopwatchProvider<long> stopwatchProvider;
        internal readonly IAsyncBlocker asyncBlocker;

        protected RateLimiterBase(IStopwatchProvider<long> stopwatchProvider)
            : this(stopwatchProvider, null)
        {
        }

        internal RateLimiterBase(
            IStopwatchProvider<long> stopwatchProvider,
            IAsyncBlocker asyncBlocker)
        {
            Contract.Requires(stopwatchProvider != null);
            this.stopwatchProvider = stopwatchProvider;
            this.asyncBlocker = asyncBlocker ?? TaskDelayAsyncBlocker.Instance;
        }

        public double PermitsPerSecond
        {
            get
            {
                semaphoreSlim.Wait();
                try
                {
                    return DoGetRate();
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            set
            {
                Contract.Requires(value > 0 && !Double.IsNaN(value));
                semaphoreSlim.Wait();
                try
                {
                    DoSetRate(value, stopwatchProvider.GetTimestamp());
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }

        public TimeSpan Acquire()
        {
            return Acquire(1);
        }

        public Task<TimeSpan> AcquireAsync()
        {
            return AcquireAsync(CancellationToken.None);
        }

        public Task<TimeSpan> AcquireAsync(CancellationToken cancellationToken)
        {
            return AcquireAsync(1, cancellationToken);
        }

        public TimeSpan Acquire(int permits)
        {
            return AcquireAsync(permits).GetAwaiter().GetResult();
        }

        public Task<TimeSpan> AcquireAsync(int permits)
        {
            return AcquireAsync(permits, CancellationToken.None);
        }

        public async Task<TimeSpan> AcquireAsync(int permits, CancellationToken cancellationToken)
        {
            var waitTimeout = await ReserveAsync(permits, cancellationToken);
            await asyncBlocker.WaitAsync(waitTimeout, cancellationToken);
            return waitTimeout;
        }

        public bool TryAcquire()
        {
            return TryAcquire(1, TimeSpan.Zero);
        }

        public Task<bool> TryAcquireAsync()
        {
            return TryAcquireAsync(CancellationToken.None);
        }

        public Task<bool> TryAcquireAsync(CancellationToken cancellationToken)
        {
            return TryAcquireAsync(1, TimeSpan.Zero, cancellationToken);
        }

        public bool TryAcquire(int permits)
        {
            return TryAcquire(permits, TimeSpan.Zero);
        }

        public Task<bool> TryAcquireAsync(int permits)
        {
            return TryAcquireAsync(permits, CancellationToken.None);
        }

        public Task<bool> TryAcquireAsync(int permits, CancellationToken cancellationToken)
        {
            return TryAcquireAsync(permits, TimeSpan.Zero, cancellationToken);
        }

        public bool TryAcquire(TimeSpan timeout)
        {
            return TryAcquire(1, timeout);
        }

        public Task<bool> TryAcquireAsync(TimeSpan timeout)
        {
            return TryAcquireAsync(timeout, CancellationToken.None);
        }

        public Task<bool> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryAcquireAsync(1, timeout, cancellationToken);
        }

        public bool TryAcquire(int permits, TimeSpan timeout)
        {
            return TryAcquireAsync(permits, timeout, CancellationToken.None).GetAwaiter().GetResult();
        }

        public Task<bool> TryAcquireAsync(int permits, TimeSpan timeout)
        {
            return TryAcquireAsync(permits, timeout, CancellationToken.None);
        }

        public async Task<bool> TryAcquireAsync(int permits, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Contract.Requires(permits > 0);

            TimeSpan waitTimeout;
            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var nowTimestamp = stopwatchProvider.GetTimestamp();
                if (!CanAcquire(nowTimestamp, timeout))
                {
                    return false;
                }
                else
                {
                    waitTimeout = ReserveAndGetWaitLength(permits, nowTimestamp);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }

            await asyncBlocker.WaitAsync(waitTimeout, cancellationToken);
            return true;
        }

        public TimeSpan Reserve(int permits)
        {
            return ReserveAsync(permits).GetAwaiter().GetResult();
        }

        public Task<TimeSpan> ReserveAsync(int permits)
        {
            return ReserveAsync(permits, CancellationToken.None);
        }

        public async Task<TimeSpan> ReserveAsync(int permits, CancellationToken cancellationToken)
        {
            Contract.Requires(permits > 0);
            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                return ReserveAndGetWaitLength(permits, stopwatchProvider.GetTimestamp());
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public TimeSpan Query(int permits)
        {
            return QueryAsync(permits).GetAwaiter().GetResult();
        }

        public Task<TimeSpan> QueryAsync(int permits)
        {
            return QueryAsync(permits, CancellationToken.None);
        }

        public async Task<TimeSpan> QueryAsync(int permits, CancellationToken cancellationToken)
        {
            Contract.Requires(permits > 0);
            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var nowTimestamp = stopwatchProvider.GetTimestamp();
                return stopwatchProvider.ParseDuration(
                    QueryEarliestAvailable(nowTimestamp),
                    nowTimestamp);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        protected abstract double DoGetRate();

        protected abstract void DoSetRate(double permitsPerSecond, long nowTimestamp);

        protected TimeSpan ReserveAndGetWaitLength(int permits, long nowTimestamp)
        {
            var momentAvailable = stopwatchProvider.ParseDuration(
                ReserveEarliestAvailable(permits, nowTimestamp),
                nowTimestamp);
            return momentAvailable.Ticks > 0 ? momentAvailable : TimeSpan.Zero;
        }

        protected bool CanAcquire(long nowTimestamp, TimeSpan timeout)
        {
            var momentAvailable = stopwatchProvider.ParseDuration(
                QueryEarliestAvailable(nowTimestamp),
                nowTimestamp);
            return momentAvailable <= timeout;
        }

        protected abstract long QueryEarliestAvailable(long nowTimestamp);

        protected abstract long ReserveEarliestAvailable(int permits, long nowTimestamp);
    }
}
