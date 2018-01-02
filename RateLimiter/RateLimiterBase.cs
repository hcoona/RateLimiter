using System;
using System.Threading;
#if !NET20
using System.Threading.Tasks;
#endif
using Clocks;

namespace RateLimiter
{
    public abstract class RateLimiterBase : IRateLimiter
    {
        protected readonly IStopwatchProvider<long> stopwatchProvider;
#if NET20
        protected readonly object lockObject = new object();
#else
        protected readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        internal readonly IAsyncBlocker asyncBlocker;
#endif

        protected RateLimiterBase(IStopwatchProvider<long> stopwatchProvider)
#if !NET20
            : this(stopwatchProvider, null)
        {
        }

        internal RateLimiterBase(
            IStopwatchProvider<long> stopwatchProvider,
            IAsyncBlocker asyncBlocker)
        {
            this.asyncBlocker = asyncBlocker ?? TaskDelayAsyncBlocker.Instance;
#else
        {
#endif
            this.stopwatchProvider = stopwatchProvider ?? throw new ArgumentNullException(nameof(stopwatchProvider));
        }

    public double PermitsPerSecond
        {
            get
            {
#if NET20
                Monitor.Enter(lockObject);
#else
                semaphoreSlim.Wait();
#endif
                try
                {
                    return DoGetRate();
                }
                finally
                {
#if NET20
                    Monitor.Exit(lockObject);
#else
                    semaphoreSlim.Release();
#endif
                }
            }
            set
            {
                if (!(value > 0 && !Double.IsNaN(value)))
                    throw new ArgumentOutOfRangeException(nameof(PermitsPerSecond));

#if NET20
                Monitor.Enter(lockObject);
#else
                semaphoreSlim.Wait();
#endif
                try
                {
                    DoSetRate(value, stopwatchProvider.GetTimestamp());
                }
                finally
                {
#if NET20
                    Monitor.Exit(lockObject);
#else
                    semaphoreSlim.Release();
#endif
                }
            }
        }

        public TimeSpan Acquire()
        {
            return Acquire(1);
        }

#if !NET20
        public Task<TimeSpan> AcquireAsync()
        {
            return AcquireAsync(CancellationToken.None);
        }

        public Task<TimeSpan> AcquireAsync(CancellationToken cancellationToken)
        {
            return AcquireAsync(1, cancellationToken);
        }
#endif

        public TimeSpan Acquire(int permits)
#if !NET20
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
#else
        {
            var waitTimeout = Reserve(permits);
            Thread.Sleep(waitTimeout);
            return waitTimeout;
        }
#endif

        public bool TryAcquire()
        {
            return TryAcquire(1, TimeSpan.Zero);
        }

#if !NET20
        public Task<bool> TryAcquireAsync()
        {
            return TryAcquireAsync(CancellationToken.None);
        }

        public Task<bool> TryAcquireAsync(CancellationToken cancellationToken)
        {
            return TryAcquireAsync(1, TimeSpan.Zero, cancellationToken);
        }
#endif

        public bool TryAcquire(int permits)
        {
            return TryAcquire(permits, TimeSpan.Zero);
        }

#if !NET20
        public Task<bool> TryAcquireAsync(int permits)
        {
            return TryAcquireAsync(permits, CancellationToken.None);
        }

        public Task<bool> TryAcquireAsync(int permits, CancellationToken cancellationToken)
        {
            return TryAcquireAsync(permits, TimeSpan.Zero, cancellationToken);
        }
#endif

        public bool TryAcquire(TimeSpan timeout)
        {
            return TryAcquire(1, timeout);
        }

#if !NET20
        public Task<bool> TryAcquireAsync(TimeSpan timeout)
        {
            return TryAcquireAsync(timeout, CancellationToken.None);
        }

        public Task<bool> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryAcquireAsync(1, timeout, cancellationToken);
        }
#endif

        public bool TryAcquire(int permits, TimeSpan timeout)
#if !NET20
        {
            return TryAcquireAsync(permits, timeout, CancellationToken.None).GetAwaiter().GetResult();
        }

        public Task<bool> TryAcquireAsync(int permits, TimeSpan timeout)
        {
            return TryAcquireAsync(permits, timeout, CancellationToken.None);
        }

        public async Task<bool> TryAcquireAsync(int permits, TimeSpan timeout, CancellationToken cancellationToken)
#endif
        {
            if (!(permits > 0)) throw new ArgumentOutOfRangeException(nameof(permits));

            TimeSpan waitTimeout;
#if NET20
            Monitor.Enter(lockObject);
#else
            await semaphoreSlim.WaitAsync(cancellationToken);
#endif
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
#if NET20
                Monitor.Exit(lockObject);
#else
                semaphoreSlim.Release();
#endif
            }

#if NET20
            Thread.Sleep(waitTimeout);
#else
            await asyncBlocker.WaitAsync(waitTimeout, cancellationToken);
#endif
            return true;
        }

        public TimeSpan Reserve(int permits)
#if !NET20
        {
            return ReserveAsync(permits).GetAwaiter().GetResult();
        }

        public Task<TimeSpan> ReserveAsync(int permits)
        {
            return ReserveAsync(permits, CancellationToken.None);
        }

        public async Task<TimeSpan> ReserveAsync(int permits, CancellationToken cancellationToken)
#endif
        {
            if (!(permits > 0)) throw new ArgumentOutOfRangeException(nameof(permits));

#if NET20
            Monitor.Enter(lockObject);
#else
            await semaphoreSlim.WaitAsync(cancellationToken);
#endif
            try
            {
                return ReserveAndGetWaitLength(permits, stopwatchProvider.GetTimestamp());
            }
            finally
            {
#if NET20
                Monitor.Exit(lockObject);
#else
                semaphoreSlim.Release();
#endif
            }
        }

        public TimeSpan Query(int permits)
#if !NET20
        {
            return QueryAsync(permits).GetAwaiter().GetResult();
        }

        public Task<TimeSpan> QueryAsync(int permits)
        {
            return QueryAsync(permits, CancellationToken.None);
        }

        public async Task<TimeSpan> QueryAsync(int permits, CancellationToken cancellationToken)
#endif
        {
            if (!(permits > 0)) throw new ArgumentOutOfRangeException(nameof(permits));

#if NET20
            Monitor.Enter(lockObject);
#else
            await semaphoreSlim.WaitAsync(cancellationToken);
#endif
            try
            {
                var nowTimestamp = stopwatchProvider.GetTimestamp();
                return stopwatchProvider.ParseDuration(
                    QueryEarliestAvailable(nowTimestamp),
                    nowTimestamp);
            }
            finally
            {
#if NET20
                Monitor.Exit(lockObject);
#else
                semaphoreSlim.Release();
#endif
            }
        }

        protected abstract double DoGetRate();

        protected abstract void DoSetRate(double permitsPerSecond, long nowTimestamp);

        protected TimeSpan ReserveAndGetWaitLength(int permits, long nowTimestamp)
        {
            var momentAvailable = stopwatchProvider.ParseDuration(
                nowTimestamp,
                ReserveEarliestAvailable(permits, nowTimestamp));
            return momentAvailable.Ticks > 0 ? momentAvailable : TimeSpan.Zero;
        }

        protected bool CanAcquire(long nowTimestamp, TimeSpan timeout)
        {
            var momentAvailable = stopwatchProvider.ParseDuration(
                nowTimestamp,
                QueryEarliestAvailable(nowTimestamp));
            return momentAvailable <= timeout;
        }

        protected abstract long QueryEarliestAvailable(long nowTimestamp);

        protected abstract long ReserveEarliestAvailable(int permits, long nowTimestamp);
    }
}
