using Fuxion.Threading.Tasks;

namespace Fuxion;

public class AverageTimeProvider : ITimeProvider
{
	readonly Random       ran = new((int)DateTime.Now.Ticks);
	public   ILogger?     Logger                    { get; set; }
	List<Entry>           Providers                 { get; }      = new();
	public int            RandomizedProvidersPerTry { get; set; } = 5;
	public int            MaxFailsPerTry            { get; set; } = 4;
	public DateTime       Now()                     => GetUtc().ToLocalTime();
	public DateTimeOffset NowOffsetted()            => GetUtc().ToLocalTime();
	public DateTime       UtcNow()                  => GetUtc();
	public DateTime GetUtc()
	{
		Logger?.LogInformation($"Get UTC time using {RandomizedProvidersPerTry} randomized servers with a maximum of {MaxFailsPerTry} fails.");
		var res = TaskManager.StartNew(() =>
		{
			if (Providers.Count(p => p.IsRandomized) < RandomizedProvidersPerTry) throw new($"At least {RandomizedProvidersPerTry} providers must be added");
			var ents = Providers.TakeRandomly(RandomizedProvidersPerTry, ran).ToList();
			Logger?.LogDebug($@"Selected servers: {ents.Aggregate("", (a, c) => a + "\r\n - " + c.Provider)}");
			foreach (var en in ents) en.Task = TaskManager.StartNew(p => p.Provider.UtcNow(), en);
			foreach (var en in ents)
				if (en.Task?.Exception == null)
					Logger?.LogInformation($"Provider '{en.Provider}' was a result: " + en.Task?.Result);
				else
					Logger?.LogWarning($"Provider '{en.Provider}' failed with error '{en.Task.Exception.GetType().Name}': " + en.Task.Exception.Message, en.Task.Exception);
			var fails = ents.Where(p => p.Task?.Exception != null).ToList();
			if (fails.Count > MaxFailsPerTry) throw new($"Failed {fails.Count} providers when the maximun to fail is {MaxFailsPerTry}");
			var r = ents.Where(en => en.Task?.Exception == null).Select(en => en.Task?.Result).RemoveNulls().RemoveOutliers().AverageDateTime();
			Logger?.LogInformation("Result: " + r);
			return r;
		});
		return res.Result;
	}
	public void AddProvider(ITimeProvider provider, bool isRandomized = true) => Providers.Add(new(isRandomized, provider));

	class Entry
	{
		public Entry(bool isRandomized, ITimeProvider provider)
		{
			IsRandomized = isRandomized;
			Provider     = provider;
		}
		public          ITimeProvider   Provider     { get; }
		public          bool            IsRandomized { get; }
		internal        Task<DateTime>? Task         { get; set; }
		public override string          ToString()   => Provider.ToString() ?? "";
	}
}