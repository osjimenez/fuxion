using Fuxion.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class AntiBackTimeProvider : ITimeProvider
    {
        public AntiBackTimeProvider(params StorageTimeProvider[] storages)
        {
            this.storages = storages;
        }

        StorageTimeProvider[] storages;

        public ILog Log { get; set; }
        public ITimeProvider TimeProvider { get; set; } = new DefaultTimeProvider();
        
        private DateTime GetUtc()
        {
            var now = TimeProvider.UtcNow();
            var last = storages.Select(s => {
                try
                {
                    return (DateTime?)s.UtcNow();
                }catch
                {
                    return null;
                }
                }).DefaultIfEmpty().Min();
            if (last == null)
                throw new NoStoredTimeValueException();
            if (now < last)
                throw new BackTimeException();
            foreach (var s in storages)
            {
                try
                {
                    s.SaveUtcTime(now);
                }
                catch (Exception ex) { Log?.Error($"Error '{ex.GetType().Name}' saving storage: {ex.Message}", ex); }
            }
            return now;
        }
        public void SetValue(DateTime value)
        {
            foreach (var s in storages)
            {
                try
                {
                    s.SaveUtcTime(value);
                }
                catch (Exception ex) { Log?.Error($"Error '{ex.GetType().Name}' saving storage: {ex.Message}", ex); }
            }
        }
        public DateTime Now() { return GetUtc().ToLocalTime(); }
        public DateTimeOffset NowOffsetted() { return GetUtc().ToLocalTime(); }
        public DateTime UtcNow() { return GetUtc(); }
    }
    public abstract class StorageTimeProvider : ITimeProvider
    {
        public abstract void SaveUtcTime(DateTime time);
        public abstract DateTime GetUtcTime();
        public DateTime Now() { return GetUtcTime().ToLocalTime(); }
        public DateTimeOffset NowOffsetted() { return GetUtcTime().ToLocalTime(); }
        public DateTime UtcNow() { return GetUtcTime(); }
    }
    public class BackTimeException : FuxionException { }
    public class NoStoredTimeValueException : FuxionException { }
}
