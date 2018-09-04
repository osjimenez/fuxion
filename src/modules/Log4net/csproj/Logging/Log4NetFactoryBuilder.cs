using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Logging
{
	public class Log4NetFactoryBuilder
	{
		List<(IAppender Appender, ILayout Layout, Level Level)> list = new List<(IAppender Appender, ILayout Layout, Level Level)>();
		string configurationFilePath;
		bool avoidConfigurationFile;
		Level rootLevel;
		public Log4NetFactoryBuilder WithConfigurationFile(string configurationFilePath)
		{
			if (avoidConfigurationFile) throw new InvalidStateException("If uses 'WithoutDefaultConfigurationFile' cannot use 'WithConfigurationFile'");
			this.configurationFilePath = configurationFilePath;
			return this;
		}
		public Log4NetFactoryBuilder WithoutDefaultConfigurationFile()
		{
			if (!string.IsNullOrWhiteSpace(configurationFilePath)) throw new InvalidStateException("If uses 'WithConfigurationFile' cannot use 'WithoutDefaultConfigurationFile'");
			avoidConfigurationFile = true;
			return this;
		}
		public Log4NetFactoryBuilder RootLevel(Level level)
		{
			rootLevel = level;
			return this;
		}
		public Log4NetFactoryBuilder AddAppender(IAppender appender, ILayout layout)
		{
			list.Add((appender, layout, null));
			return this;
		}
		public Log4NetFactoryBuilder AddAppender(IAppender appender, ILayout layout, Level level)
		{
			list.Add((appender, layout, level));
			return this;
		}
		internal void Configure()
		{
			Hierarchy repository = null;
			if (avoidConfigurationFile)
			{
				BasicConfigurator.Configure();
				repository = (Hierarchy)log4net.LogManager.GetRepository();
				repository.Root.RemoveAllAppenders();
			}
			else
			{
				var configPath = "";
				if (!string.IsNullOrWhiteSpace(configurationFilePath))
					configPath = configurationFilePath;
				else
				{
					Assembly ass = Assembly.GetEntryAssembly() ?? Assembly.GetAssembly(typeof(Log4netFactory));
					string assemblyBasedPath = $"{ass.Location}.log4net";
					if (File.Exists(assemblyBasedPath))
						configPath = assemblyBasedPath;
					else
						configPath = Path.Combine(Path.GetDirectoryName(ass.Location), "log4net.config");
				}
				XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
				repository = (Hierarchy)log4net.LogManager.GetRepository();
			}
			if (rootLevel != null)
				repository.Root.Level = rootLevel;
			foreach (var (appender, layout, level) in list)
			{
				if (appender is AppenderSkeleton app)
				{
					app.Layout = layout;
					if (level != null)
						app.Threshold = level;
				}
				if (appender is IOptionHandler aop) aop.ActivateOptions();
				if (layout is IOptionHandler lop) lop.ActivateOptions();
				repository.Root.AddAppender(appender);
			}
		}
	}
}
