using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class LocalMachinneTimeProvider : ITimeProvider
    {
        public DateTime Now() { return DateTime.Now; }
        public DateTimeOffset NowOffsetted() { return DateTimeOffset.Now; }
        public DateTime UtcNow() { return DateTime.UtcNow; }
    }
}
