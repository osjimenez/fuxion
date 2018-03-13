using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fuxion.Threading.Tasks
{
    internal interface ITaskManagerEntry
    {
		ConcurrencyProfile ConcurrencyProfile { get; }
		Delegate Delegate { get; }
		Task Task { get; }
        bool IsCancellationRequested { get; }
        TaskScheduler TaskScheduler { get; }
        TaskCreationOptions TaskCreationOptions { get; }
        event EventHandler CancelRequested;
        void Cancel();
        void Start();
        CancellationTokenSource CancellationTokenSource { get; }
	}
}
