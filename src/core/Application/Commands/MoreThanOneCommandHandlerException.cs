using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Application.Commands
{
	public class MoreThanOneCommandHandlerException : FuxionException
	{
		public MoreThanOneCommandHandlerException(string msg) : base(msg) { }
	}
}
