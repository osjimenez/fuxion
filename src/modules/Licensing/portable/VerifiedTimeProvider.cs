using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class VerifiedTimeProvider : IAsyncTimeProvider
    {
        Random ran = new Random((int)DateTime.Now.Ticks);

        public ILog Log { get; set; }
        //public ITimeProvider BaseProvider { get; set; }
        List<Entry> Providers { get; set; } = new List<Entry>();
        public int RandomizedProvidersPerTry { get; set; } = 5;
        public int MaxFailsPerTry { get; set; } = 4;
        //public DateTime LastVerifiedValue { get; set; }
        //TimeSpan VerificationExpirationInterval { get; set; }
        public async Task<DateTime> GetUtcAsync()
        {
            await Task.Delay(500).ConfigureAwait(false);
            Log?.Notice($"Get UTC time using {RandomizedProvidersPerTry} randomized servers with a maximum of {MaxFailsPerTry} fails.");
            var res = await TaskManager.StartNew(async () =>
            {
                if (Providers.Count(p => p.IsRandomized) < RandomizedProvidersPerTry)
                    throw new Exception($"At least {RandomizedProvidersPerTry} providers must be added");

                //var ents = Providers.ToList();
                var ents = Providers.TakeRandomly(RandomizedProvidersPerTry, ran).ToList();
                Log?.Debug($@"Selected servers: {ents.Aggregate("", (a, c) => a + "\r\n - " + c.Provider)}");

                foreach (var en in ents)
                    en.Task = TaskManager.StartNew(p => p.Provider.UtcNow(), en);
                Log?.Debug($@"Selected servers: {ents.Aggregate("", (a, c) => a + "\r\n - " + c.Provider)}");
                DateTime[] results = null;
                try
                {
                    //Task.WaitAll(ents.Select(en => en.Task).ToArray());
                    results = await Task.WhenAll(ents.Select(en => en.Task).ToArray()).ConfigureAwait(false);
                }
                catch (Exception ex)
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
                var fails = ents.Where(p => !p.CanFail && p.Task.Exception != null).ToList();
                if (fails.Count > 0)
                {
                    foreach (var fail in fails)
                        Log?.Error($"The provider '{fail.Provider}' fail, and is marked as 'cannot fail'.");
                    throw new Exception("A provider that could not fail eventually failed");
                }

                var failCount = fails.Count;
                if (failCount > MaxFailsPerTry)
                    throw new Exception($"Failed {failCount} providers when the maximun to fail is {MaxFailsPerTry}");

                var r = ents
                    .Where(en => en.Task.Exception == null)
                    .Select(en => en.Task.Result)
                    .RemoveOutliers(m => Log.Debug(m))
                    .AverageDateTime();
                Log?.Notice("Result: " + r);
                return r;
            }).ConfigureAwait(false);
            return res;
        }
        private Task<DateTime> GetUtcAsync2()
        {
            return TaskManager.StartNew(() =>
            {
                Log?.Notice($"Get UTC time using {RandomizedProvidersPerTry} randomized servers with a maximum of {MaxFailsPerTry} fails.");
                if (Providers.Count(p => p.IsRandomized) < RandomizedProvidersPerTry)
                    throw new Exception($"At least {RandomizedProvidersPerTry} providers must be added");

                var pros = Providers
                    .Where(p => p.IsRandomized).TakeRandomly(RandomizedProvidersPerTry, ran)
                    .Concat(Providers.Where(p => !p.IsRandomized))
                    .ToList(); // ToList() is important because if i not execute the expression, the list will be randomized each access
                Log?.Debug($@"Selected servers: {pros.Aggregate("", (a, c) => a + "\r\n - " + c.Provider)}");

                try
                {
                    foreach (var pro in pros)
                        pro.Task = TaskManager.StartNew(p => p.Provider.UtcNow(), pro);
                    Task.WaitAll(pros.Select(p => p.Task).ToArray());
                }
                catch (Exception ex)
                {
                    Log?.Error($"Error in a provider {ex.GetType().Name}: " + ex.Message, ex);
                }
                var fails = pros.Where(p => !p.CanFail && p.Task.Exception != null);
                if (fails.Any())
                {
                    foreach (var fail in fails)
                        Log?.Error($"The provider '{fail.Provider}' fail, and is marked as 'cannot fail'.");
                    throw new Exception("A provider that could not fail eventually failed");
                }

                var failCount = pros.Count(p => p.Task.Exception != null);
                if (failCount > MaxFailsPerTry)
                    throw new Exception($"Failed {failCount} providers when the maximun to fail is {MaxFailsPerTry}");
                try
                {
                    return pros.Where(p => p.Task.Exception == null).Select(p => p.Task.Result).AverageDateTime();
                }
                catch (Exception ex)
                {
                    return DateTime.MinValue;
                }
            });
        }

        public void AddProvider(ITimeProvider provider, bool canFail = true, bool isRandomized = true)
        {
            Providers.Add(new Entry
            {
                CanFail = canFail,
                IsRandomized = isRandomized,
                Provider = provider
            });
        }
        public async Task<DateTime> NowAsync() { return (await GetUtcAsync()).ToLocalTime(); }
        public async Task<DateTimeOffset> NowOffsettedAsync() { return (await GetUtcAsync()).ToLocalTime(); }
        public Task<DateTime> UtcNowAsync() { return GetUtcAsync(); }
        class Entry
        {
            public ITimeProvider Provider { get; set; }
            public bool CanFail { get; set; }
            public bool IsRandomized { get; set; }
            public Task<DateTime> Task { get; set; }
        }
    }
}
