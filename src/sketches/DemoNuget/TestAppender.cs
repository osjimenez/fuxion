using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoNuget
{
	public class TestAppender : AppenderSkeleton
	{
		protected override void Append(LoggingEvent loggingEvent)
		{
			Console.WriteLine("OOOOOOOOO - " + loggingEvent.MessageObject);
		}
	}
}
