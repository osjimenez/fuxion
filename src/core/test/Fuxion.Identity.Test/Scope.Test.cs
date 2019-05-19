using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Mocks;
using Fuxion.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Identity.Test
{
    public class ScopeTest : BaseTest
    {
        public ScopeTest(ITestOutputHelper helper) : base(helper) { }
        [Fact(DisplayName = "Scope - Null discriminator validation")]
        public void Validate_WrongName()
        {
			Assert.False(new ScopeDao("", "", null!, ScopePropagation.ToMe).IsValid());
            //Assert.IsTrue(new Scope(null, ScopePropagation.ToMe).IsValid());
        }
    }
}
