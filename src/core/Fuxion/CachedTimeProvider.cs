using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Fuxion
{
	public class CachedTimeProvider : ITimeProvider
	{
		public CachedTimeProvider(ITimeProvider timeProvider) => TimeProvider = timeProvider;

		private DateTime cachedValue;
		private readonly Stopwatch stopwatch = new();

		public ILogger? Logger { get; set; }
		public ITimeProvider TimeProvider { get; set; }
		public TimeSpan ExpirationInterval { get; set; } = TimeSpan.FromSeconds(5);

		private DateTime GetUtc(out bool fromCache)
		{
			Logger?.LogInformation($"Getting time from {nameof(CachedTimeProvider)} ...");
			if (!stopwatch.IsRunning || stopwatch.Elapsed > ExpirationInterval)
			{
				if (!stopwatch.IsRunning)
					Logger?.LogInformation("There is no data stored in cache yet, time provider will be used");
				else
					Logger?.LogInformation("Cache expired, time provider will be used");
				fromCache = false;
				var res = TimeProvider.UtcNow();
				stopwatch.Restart();
				cachedValue = res;
			}
			else
			{
				Logger?.LogInformation($"Result is cache + elapsed = {cachedValue} + {stopwatch.Elapsed}");
				fromCache = true;
			}
			return cachedValue.Add(stopwatch.Elapsed);
		}

		public DateTime Now()
		{
			return Now(out _);
		}
		public DateTime Now(out bool fromCache) => GetUtc(out fromCache).ToLocalTime();
		public DateTimeOffset NowOffsetted()
		{
			return NowOffsetted(out _);
		}
		public DateTimeOffset NowOffsetted(out bool fromCache) => GetUtc(out fromCache).ToLocalTime();
		public DateTime UtcNow()
		{
			return UtcNow(out _);
		}
		public DateTime UtcNow(out bool fromCache) => GetUtc(out fromCache);
	}
}
