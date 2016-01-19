using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    class Scope : IScope
    {
        public Scope(IDiscriminator discriminator, ScopePropagation propagation)
        {
            Discriminator = discriminator;
            Propagation = propagation;
        }
        public IDiscriminator Discriminator { get; set; }
        public ScopePropagation Propagation { get; set; }
    }
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
