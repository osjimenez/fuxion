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
    [FactoryDefaultImplementation(typeof(LocalMachinneTimeProvider))]
    public interface ITimeProvider
    {
        DateTime Now();
        DateTimeOffset NowOffsetted();
        DateTime UtcNow();
    }
}