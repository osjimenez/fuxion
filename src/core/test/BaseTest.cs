using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class BaseTest
    {
        public BaseTest(ITestOutputHelper output)
        {
            this.output = output;
            Printer.WriteLineAction = m =>
            {
                try
                {
                    output.WriteLine(m);
                    Debug.WriteLine(m);
                }
                catch { }
            };
        }
        ITestOutputHelper output;
    }
}
