using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Fuxion.Identity.Test.Mocks;

namespace Fuxion.Identity.Test
{
    [TestClass]
    public class EntityTest : BaseTestClass
    {
        [TestMethod]
        public void GetDiscriminators()
        {
            Assert.Fail("Pending refactoring");
            //var entity = new Mocks.Entity { DisId = "test" };
            //var type = typeof(Mocks.Entity);
            //var disProps = type.GetProperties()
            //    //.Where(p => p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, true) != null)
            //    .Select(p =>
            //    {
            //        var proAtt = p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, true);
            //        var disAtt = proAtt?.Type.GetCustomAttribute<DiscriminatorAttribute>(true, false, true);
            //        return new
            //        {
            //            Property = p,
            //            PropertyAttribute = proAtt,
            //            DiscriminatorAttribute = disAtt,
            //        };
            //    })
            //    .Where(p => p.PropertyAttribute != null);
            //Debug.WriteLine($"El tipo '{nameof(Mocks.Entity)}' tiene los siguientes discriminadores:");
            //foreach(var p in disProps)
            //{
            //    Debug.WriteLine($"   La propiedad '{p.Property.Name}' con el valor '{p.Property.GetValue(entity)}' discrimina por '{p.DiscriminatorAttribute.Key}' con el tipo '{p.PropertyAttribute.Type.Name}'");
            //}
        }
    }
}
