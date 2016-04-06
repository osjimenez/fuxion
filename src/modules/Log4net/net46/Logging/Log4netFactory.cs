using System;
using System.IO;
using System.Reflection;
using log4net.Config;
using log4net.Core;
using System.Diagnostics;

namespace Fuxion.Logging
{
	public class Log4netFactory : MarshalByRefObject,  ILogFactory
	{
        public Log4netFactory() : this(null) { }
        public Log4netFactory(string configFilePath = null)
        {
            if (configFilePath == null)
                this.configFilePath = Path.GetDirectoryName(GetType().Assembly.Location) + $@"\{configFilePath}";
            else
                this.configFilePath = configFilePath;
        }
        string configFilePath;
        private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));
		private static ILoggerWrapper WrapperCreationHandler(ILogger logger) { return new Log4netLog(logger); }
		private static ILog4netLog WrapLogger(ILogger logger) { return (ILog4netLog)s_wrapperMap.GetWrapper(logger); }
        public ILog Create(Type declaringType) { return WrapLogger(LoggerManager.GetLogger(Assembly.GetCallingAssembly(), declaringType.FullName)); }
        public void Initialize() { XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilePath)); }
	}
}
