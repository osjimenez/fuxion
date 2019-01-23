using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class MockTimeProvider : ITimeProvider
    {
        public bool MustFail { get; set; }
        public TimeSpan Offset { get; private set; }
        public void SetOffset(TimeSpan offset)
        {
            Offset = offset;
        }
        private DateTime GetUtc()
        {
            if (MustFail) throw new MockTimeProviderException();
            return DateTime.UtcNow.Add(Offset);
        }
        public DateTime Now() { return GetUtc().ToLocalTime(); }
        public DateTimeOffset NowOffsetted() { return GetUtc().ToLocalTime(); }
        public DateTime UtcNow() { return GetUtc(); }
    }
    public class MockTimeProviderException : FuxionException { }
}
