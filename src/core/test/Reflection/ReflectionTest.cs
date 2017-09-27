using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Reflection
{
    public class ReflectionTest : BaseTest
    {
        public ReflectionTest(ITestOutputHelper output) : base(output) { }
        //[Fact(DisplayName = "Reflection - Constructors")]
        //public void Constructors()
        //{
        //    var ctos = typeof(B).GetAllConstructors();
        //    Assert.Equal(2, ctos.Count());
        //    Assert.Empty(ctos.First().GetParameters());
        //    Assert.Single(ctos.Skip(1).First().GetParameters());
        //    Assert.Equal(typeof(int), ctos.Skip(1).First().GetParameters().First().ParameterType);
        //}
        //[Fact(DisplayName = "Reflection - Events")]
        //public void Events()
        //{
        //    var ctos = typeof(B).GetAllEvents();
        //    Assert.Equal(1, ctos.Count());
        //}
    }
    public class A
    {
        public A(string par) { }

        public event EventHandler E;
    }
    public class B : A
    {
        private B() : base(null) { }
        protected B(int par) : base(par.ToString()) { }
    }
}
