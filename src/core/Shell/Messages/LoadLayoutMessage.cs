using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.Messages
{
	internal class LoadLayoutMessage
	{
		public LoadLayoutMessage(Stream layoutFileStream) => LayoutFileStream = layoutFileStream;
		public Stream LayoutFileStream { get; set; }
	}
}
