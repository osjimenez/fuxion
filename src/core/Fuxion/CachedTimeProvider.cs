using System.Diagnostics;

namespace Fuxion;

public class CachedTimeProvider : ITimeProvider
{
	public CachedTimeProvider(ITimeProvider timeProvider) => TimeProvider = timeProvider;
	readonly Stopwatch stopwatch = new();
	DateTime cachedValue;
	public ILogger? Logger { get; set; }
	public ITimeProvider TimeProvider { get; set; }
	public TimeSpan ExpirationInterval { get; set; } = TimeSpan.FromSeconds(5);
	public DateTime Now() => Now(out _);
	public DateTimeOffset NowOffsetted() => NowOffsetted(out _);
	public DateTime UtcNow() => UtcNow(out _);
	DateTime GetUtc(out bool fromCache)
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
		} else
		{
			Logger?.LogInformation($"Result is cache + elapsed = {cachedValue} + {stopwatch.Elapsed}");
			fromCache = true;
		}
		return cachedValue.Add(stopwatch.Elapsed);
	}
	public DateTime Now(out bool fromCache) => GetUtc(out fromCache).ToLocalTime();
	public DateTimeOffset NowOffsetted(out bool fromCache) => GetUtc(out fromCache).ToLocalTime();
	public DateTime UtcNow(out bool fromCache) => GetUtc(out fromCache);
}