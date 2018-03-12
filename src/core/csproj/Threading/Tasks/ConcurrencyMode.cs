using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Threading.Tasks
{
	public enum ConcurrencyMode
	{
		/// <summary>
		/// All calls will be execute in parallel, at the moment of each call. This is the default behavior.
		/// </summary>
		Parallely,
		/// <summary>
		/// All calls will be executed one by one sequentially in a queue, no parallelization occur.
		/// </summary>
		Sequentially,

		/// <summary>
		/// All calls will be executed one by one sequentially in a queue, no parallelization occur. When a call ends, only last call in the queue will be executed, the others will be cancelled.
		/// </summary>
		SequentiallyAndExecuteOnlyLast,

		
		CancelPrevious,
		CancelAndWaitPrevious,


	}
	public struct ConcurrencyProfile
	{
		public static readonly ConcurrencyProfile RunInParallel = new ConcurrencyProfile();
		public static readonly ConcurrencyProfile RunSequentially = new ConcurrencyProfile { Sequentially = true };
		public static readonly ConcurrencyProfile RunSequentiallyAndExecuteOnlyLast = new ConcurrencyProfile { Sequentially = true, ExecuteOnlyLast = true };

		public bool Sequentially { get; set; }
		public bool ExecuteOnlyLast { get; set; }
		public bool CancelPrevious { get; set; }
		public bool WaitForCancelPrevious { get; set; }
	}
}