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
        Task Task { get; }
        bool IsCancellationRequested { get; }
        TaskScheduler TaskScheduler { get; }
        TaskCreationOptions TaskCreationOptions { get; }
        AutoResetEvent AutoResetEvent { get; }
        bool IsSleeping { get; set; }
        event EventHandler Canceled;
        void Cancel();
        void Start();
    }
}
