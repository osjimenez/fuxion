using Fuxion.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class CachedTimeProvider : ITimeProvider
    {
        public CachedTimeProvider(ITimeProvider timeProvider) { TimeProvider = timeProvider; }

        DateTime cachedValue;
        Stopwatch stopwatch = new Stopwatch();

        public ILog Log { get; set; }
        public ITimeProvider TimeProvider { get; set; }
        public TimeSpan ExpirationInterval { get; set; } = TimeSpan.FromSeconds(5);

        private DateTime GetUtc(out bool fromCache) {
            Log?.Notice($"Getting time from {nameof(CachedTimeProvider)} ...");
            if (!stopwatch.IsRunning || stopwatch.Elapsed > ExpirationInterval)
            {
                if (!stopwatch.IsRunning)
                    Log?.Info("There is no data stored in cache yet, time provider will be used");
                else
                    Log?.Info("Cache expired, time provider will be used");
                fromCache = false;
                var res = TimeProvider.UtcNow();
                stopwatch.Restart();
                cachedValue = res;
            }
            else
            {
                Log?.Info($"Result is cache + elapsed = {cachedValue} + {stopwatch.Elapsed}");
                fromCache = true;
            }
            return cachedValue.Add(stopwatch.Elapsed);
        }

        public DateTime Now()
        {
            bool fromCache;
            return Now(out fromCache);
        }
        public DateTime Now(out bool fromCache)
        {
            return GetUtc(out fromCache).ToLocalTime();
        }
        public DateTimeOffset NowOffsetted()
        {
            bool fromCache;
            return NowOffsetted(out fromCache);
        }
        public DateTimeOffset NowOffsetted(out bool fromCache)
        {
            return GetUtc(out fromCache).ToLocalTime();
        }
        public DateTime UtcNow()
        {
            bool fromCache;
            return UtcNow(out fromCache);
        }
        public DateTime UtcNow(out bool fromCache)
        {
            return GetUtc(out fromCache);
        }
    }
}
