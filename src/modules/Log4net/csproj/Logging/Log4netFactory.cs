#if NET47
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
        public Log4netFactory() { }
        string configurationFilePath;
        public string ConfigurationFilePath {
            get
            {
                if (!string.IsNullOrWhiteSpace(configurationFilePath)) return configurationFilePath;
                var ass = Assembly.GetEntryAssembly() ?? Assembly.GetAssembly(typeof(Log4netFactory));
                var assemblyBasedPath = $"{ass.Location}.log4net";
                if (File.Exists(assemblyBasedPath))
                    return assemblyBasedPath;
                return Path.Combine(Path.GetDirectoryName(ass.Location), "log4net.config");
            }
            set { configurationFilePath = value; }
        }
        private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));
		private static ILoggerWrapper WrapperCreationHandler(ILogger logger) { return new Log4netLog(logger); }
		private static ILog4netLog WrapLogger(ILogger logger) { return (ILog4netLog)s_wrapperMap.GetWrapper(logger); }
        public ILog Create(Type declaringType) { return WrapLogger(LoggerManager.GetLogger(Assembly.GetCallingAssembly(), declaringType.FullName)); }
        public void Initialize() { XmlConfigurator.ConfigureAndWatch(new FileInfo(ConfigurationFilePath)); }
	}
}
#endif