using Fuxion.Factories;
using Fuxion.Logging;
using SimpleInjector;
using System;
using Xunit;

namespace Fuxion.Log4net.Test
{
    public class Log4netFactoryTest
    {
        [Fact]
        public void Log4netFactory_ConfigFile()
        {
            Container c = new Container();
            //c.Register<ILogFactory, Log4netFactory>();
            c.Register<ILogFactory>(()=>new Log4netFactory { ConfigFilePath = "log4net.config" });
            Factory.AddToPipe(new SimpleInjectorFactory(c));
            var log = LogManager.Create<Log4netFactoryTest>();
            log.Notice("Log4netFactory_First");
            Assert.True(true);
        }
    }
}
