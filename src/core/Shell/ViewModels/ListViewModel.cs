using Fuxion.Shell.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.ViewModels
{
	public class ListViewModel<TDvo> : BaseViewModel
		where TDvo : BaseDvo
	{
		public ListViewModel(ILogger<ListViewModel<TDvo>> logger) : base(logger) { }
	}
}
