﻿using System;
using System.Collections.Generic;
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
            Printer.PrintAction = m =>
            {
                try
                {
                    output.WriteLine(m);
                }
                catch { }
            };
        }
        ITestOutputHelper output;
    }
}
