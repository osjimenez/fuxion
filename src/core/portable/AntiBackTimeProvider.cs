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
        public AntiBackTimeProvider(params StoredTimeProvider[] storedProviders)
        {
            this.providers = storedProviders;
        }

        StoredTimeProvider[] providers;

        public ILog Log { get; set; }
        public ITimeProvider TimeProvider { get; set; } = new LocalMachinneTimeProvider();
        
        private DateTime GetUtc()
        {
            var now = TimeProvider.UtcNow();
            var stored = providers.Select(s =>
            {
                try
                {
                    return (DateTime?)s.UtcNow();
                }
                catch
                {
                    return null;
                }
            }).DefaultIfEmpty().Min();
            if (stored == null)
                throw new NoStoredTimeValueException();
            if (now < stored)
                throw new BackTimeException(stored.Value, now);
            Log?.Notice("now => " + now);
            Log?.Notice("stored => " + stored);

            SetValue(now);
            return now;
        }
        public void SetValue(DateTime value)
        {
            foreach (var s in providers)
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
    public abstract class StoredTimeProvider : ITimeProvider
    {
        public abstract void SaveUtcTime(DateTime time);
        public abstract DateTime GetUtcTime();

        protected virtual string Serialize(DateTime time) { return time.ToString(); }
        protected virtual DateTime Deserialize(string value) { return DateTime.Parse(value); }

        public DateTime Now() { return GetUtcTime().ToLocalTime(); }
        public DateTimeOffset NowOffsetted() { return GetUtcTime().ToLocalTime(); }
        public DateTime UtcNow() { return GetUtcTime(); }
    }
    public class BackTimeException : FuxionException {
        public BackTimeException(DateTime storedTime, DateTime currentTime) : base($"Time stored '{storedTime}' is most recent that current time '{currentTime}'")
        {
            StoredTime = storedTime;
            currentTime = CurrentTime;
        }
        public DateTime StoredTime { get; set; }
        public DateTime CurrentTime { get; set; }
    }
    public class NoStoredTimeValueException : FuxionException {
        public NoStoredTimeValueException() : base("No value was found in the stored time providers") { }
    }
}
