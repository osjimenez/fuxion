using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class DisposableEnvelopeTest : BaseTest
    {
        public DisposableEnvelopeTest(ITestOutputHelper output) : base(output) { }
        [Fact(DisplayName = "DisposableEnvelope - Test")]
        public void Test()
        {
            using ("hola".AsDisposable(m => {
                Debug.WriteLine(m);
            }))
            {
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
        }
    }
}
