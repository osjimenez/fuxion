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
        DateTime Now();
        DateTimeOffset NowOffsetted();
        DateTime UtcNow();
    }
    public interface IAsyncTimeProvider
    {
        Task<DateTime> NowAsync();
        Task<DateTimeOffset> NowOffsettedAsync();
        Task<DateTime> UtcNowAsync();
    }
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime Now() { return DateTime.Now; }
        public DateTimeOffset NowOffsetted() { return DateTimeOffset.Now; }
        public DateTime UtcNow() { return DateTime.UtcNow; }
    }
}