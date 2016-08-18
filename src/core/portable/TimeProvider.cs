using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fuxion
{
    [FactoryDefaultImplementation(typeof(DefaultTimeProvider))]
    public interface ITimeProvider
    {
        DateTime GetUtcNow();
        DateTimeOffset GetUtcNowWithOffset();
    }
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime GetUtcNow() { return DateTime.UtcNow; }
        public DateTimeOffset GetUtcNowWithOffset() { return DateTimeOffset.Now.ToUniversalTime(); }
    }
}
