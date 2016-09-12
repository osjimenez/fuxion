using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class MockTimeProvider : ITimeProvider
    {
        public TimeSpan Offset { get; private set; }
        public void SetOffset(TimeSpan offset)
        {
            Offset = offset;
        }
        public DateTime Now()
        {
            return DateTime.Now.Add(Offset);
        }
        public DateTimeOffset NowOffsetted()
        {
            return DateTimeOffset.Now.Add(Offset);
        }
        public DateTime UtcNow()
        {
            return DateTime.UtcNow.Add(Offset);
        }
    }
}
