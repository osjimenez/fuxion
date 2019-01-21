using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class AverageTimeProvider : ITimeProvider
    {
        Random ran = new Random((int)DateTime.Now.Ticks);
        public ILog Log { get; set; }
        List<Entry> Providers { get; set; } = new List<Entry>();
        public int RandomizedProvidersPerTry { get; set; } = 5;
        public int MaxFailsPerTry { get; set; } = 4;
        public DateTime GetUtc()
        {
            Log?.Notice($"Get UTC time using {RandomizedProvidersPerTry} randomized servers with a maximum of {MaxFailsPerTry} fails.");
            var res = TaskManager.StartNew(async () =>
            {
                if (Providers.Count(p => p.IsRandomized) < RandomizedProvidersPerTry)
                    throw new Exception($"At least {RandomizedProvidersPerTry} providers must be added");
                var ents = Providers.TakeRandomly(RandomizedProvidersPerTry, ran).ToList();
                Log?.Debug($@"Selected servers: {ents.Aggregate("", (a, c) => a + "\r\n - " + c.Provider)}");

                foreach (var en in ents)
                    en.Task = TaskManager.StartNew(p => p.Provider.UtcNow(), en);
                DateTime[] results = null;
                try
                {
                    results = await Task.WhenAll(ents.Select(en => en.Task).ToArray()).ConfigureAwait(false);
                }
                catch
                {
                    Debug.WriteLine("");
                }
                foreach (var en in ents)
                {
                    if (en.Task.Exception == null)
                        Log?.Info($"Provider '{en.Provider}' was a result: " + en.Task.Result);
                    else
                        Log?.Warn($"Provider '{en.Provider}' failed with error '{en.Task.Exception.GetType().Name}': " + en.Task.Exception.Message, en.Task.Exception);
                }
                var fails = ents.Where(p => p.Task.Exception != null).ToList();
                if (fails.Count > MaxFailsPerTry)
                    throw new Exception($"Failed {fails.Count} providers when the maximun to fail is {MaxFailsPerTry}");

                var r = ents
                    .Where(en => en.Task.Exception == null)
                    .Select(en => en.Task.Result)
                    .RemoveOutliers()
                    .AverageDateTime();
                Log?.Notice("Result: " + r);
                return r;
            });
            return res.Result;
        }

        public void AddProvider(ITimeProvider provider, bool canFail = true, bool isRandomized = true)
        {
            Providers.Add(new Entry
            {
                IsRandomized = isRandomized,
                Provider = provider
            });
        }
        public DateTime Now() { return ( GetUtc()).ToLocalTime(); }
        public DateTimeOffset NowOffsetted() { return ( GetUtc()).ToLocalTime(); }
        public DateTime UtcNow() { return GetUtc(); }
        class Entry
        {
            public ITimeProvider Provider { get; set; }
            public bool IsRandomized { get; set; }
            internal Task<DateTime> Task { get; set; }
            public override string ToString() { return Provider.ToString(); }
        }
    }
}
