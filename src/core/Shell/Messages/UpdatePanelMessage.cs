using Fuxion.Shell.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Shell.Messages
{
	public class UpdatePanelMessage
	{
		public UpdatePanelMessage(BaseDvo updatedDvo)
		{
			UpdatedDvo = updatedDvo;
		}
		public BaseDvo UpdatedDvo { get; set; }
	}
}
