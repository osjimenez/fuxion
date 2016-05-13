using Fuxion.Factories;
using Fuxion.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public interface IDemo { }
    public class Demo : IDemo { }
    public class LogTest
    {
        [Fact]
        public void Demo()
        {
            var fac = new Mock<ILogFactory>();
            Factory.AddInjector(new FunctionInjector<ILogFactory>(() => new Log4netFactory()));


            Factory.AddInjector(new FunctionInjector<IDemo>(() => new Demo()));

            var demo = Factory.Get<IDemo>();
            var log = LogManager.Create<LogTest>();
            Debug.WriteLine("");
        }
    }
}
