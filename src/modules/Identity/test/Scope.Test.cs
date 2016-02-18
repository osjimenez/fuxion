using Fuxion.Identity.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    [TestClass]
    public class ScopeTest
    {
        [TestMethod]
        public void Validate_WrongName()
        {
            Assert.IsFalse(new Scope(null, ScopePropagation.ToMe).IsValid());
            //Assert.IsTrue(new Scope(null, ScopePropagation.ToMe).IsValid());
        }
    }
}
