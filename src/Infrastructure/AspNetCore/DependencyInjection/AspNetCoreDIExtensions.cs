using Fuxion.Reflection;
using Fuxion.Application.Commands;
using Fuxion.Application.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.AspNetCore.Controllers;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class AspNetCoreDIExtensions
	{
		public static IMvcBuilder AddSingularityControllers(this IMvcBuilder me) => me.AddApplicationPart(typeof(CommandController).Assembly);
	}	
}
