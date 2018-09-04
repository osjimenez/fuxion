#if (NET471)
using log4net.Config;
using log4net.Core;
using System;
using System.IO;
using System.Reflection;

namespace Fuxion.Logging
{
	public class Log4netFactory : MarshalByRefObject, ILogFactory
	{
		public Log4netFactory() { }
		//public Log4netFactory(Action postInitializedAction) : this() { this.postInitializedAction = postInitializedAction; }
		public Log4netFactory(Action<Log4NetFactoryBuilder> builderAction) : this() { builderAction(builder); }
		//private readonly Action postInitializedAction;
		private readonly Log4NetFactoryBuilder builder = new Log4NetFactoryBuilder();
		private string configurationFilePath;
		public string ConfigurationFilePath
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(configurationFilePath))
				{
					return configurationFilePath;
				}

				Assembly ass = Assembly.GetEntryAssembly() ?? Assembly.GetAssembly(typeof(Log4netFactory));
				string assemblyBasedPath = $"{ass.Location}.log4net";
				if (File.Exists(assemblyBasedPath))
				{
					return assemblyBasedPath;
				}

				return Path.Combine(Path.GetDirectoryName(ass.Location), "log4net.config");
			}
			set => configurationFilePath = value;
		}
		private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));
		private static ILoggerWrapper WrapperCreationHandler(ILogger logger) => new Log4netLog(logger);
		private static ILog4netLog WrapLogger(ILogger logger) => (ILog4netLog)s_wrapperMap.GetWrapper(logger);
		public ILog Create(Type declaringType) => WrapLogger(LoggerManager.GetLogger(Assembly.GetCallingAssembly(), declaringType.FullName));
		public ILog Create(string loggerName)=> WrapLogger(LoggerManager.GetLogger(Assembly.GetCallingAssembly(), loggerName));
		public void Initialize() => builder.Configure();
	}
}
#endif