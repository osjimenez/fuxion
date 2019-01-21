using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell
{
	public interface IMenu
	{
		object Header { get; }
		void OnClick();
	}
}
