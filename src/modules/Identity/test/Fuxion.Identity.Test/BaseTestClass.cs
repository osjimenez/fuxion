using Fuxion.Identity.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    [TestClass]
    public class BaseTestClass
    {
        public BaseTestClass() { Bootstrapper.Initialize(); }
    }
}
