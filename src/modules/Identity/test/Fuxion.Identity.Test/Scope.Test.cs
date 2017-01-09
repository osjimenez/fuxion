using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Dao;
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
    public class ScopeTest : BaseTestClass
    {
        [TestMethod]
        public void Validate_WrongName()
        {
            Assert.IsFalse(new ScopeDao { Propagation = ScopePropagation.ToMe }.IsValid());
            //Assert.IsTrue(new Scope(null, ScopePropagation.ToMe).IsValid());
        }
    }
}
