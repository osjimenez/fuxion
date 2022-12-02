namespace Fuxion.Threading.Tasks;

interface ITaskManagerEntry
{
	ConcurrencyProfile ConcurrencyProfile { get; }
	Delegate? Delegate { get; }
	Task Task { get; }
	bool IsCancellationRequested { get; }
	TaskScheduler TaskScheduler { get; }
	TaskCreationOptions TaskCreationOptions { get; }
	CancellationTokenSource CancellationTokenSource { get; }
	event EventHandler CancelRequested;
	void Cancel();
	void Start();
}