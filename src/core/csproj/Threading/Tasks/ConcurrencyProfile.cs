using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Threading.Tasks
{
	public struct ConcurrencyProfile
	{
		public static readonly ConcurrencyProfile RunInParallel = new ConcurrencyProfile();
		public static readonly ConcurrencyProfile RunSequentially = new ConcurrencyProfile { Sequentially = true };
		public static readonly ConcurrencyProfile RunSequentiallyAndExecuteOnlyLast = new ConcurrencyProfile { Sequentially = true, ExecuteOnlyLast = true };

		public bool Sequentially { get; set; }
		public bool ExecuteOnlyLast { get; set; }
		public bool CancelPrevious { get; set; }
		//public bool WaitForCancelPrevious { get; set; }
	}
}