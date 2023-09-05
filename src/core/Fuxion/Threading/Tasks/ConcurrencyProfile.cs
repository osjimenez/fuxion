namespace Fuxion.Threading.Tasks;

public struct ConcurrencyProfile
{
	public static readonly ConcurrencyProfile Default = new();
	public static readonly ConcurrencyProfile RunSequentially = new() {
		Sequentially = true
	};
	public static readonly ConcurrencyProfile RunSequentiallyAndCancelPrevious = new() {
		Sequentially = true, CancelPrevious = true
	};
	public static readonly ConcurrencyProfile RunSequentiallyAndExecuteOnlyLast = new() {
		Sequentially = true, ExecuteOnlyLast = true
	};
	public static readonly ConcurrencyProfile RunSequentiallyCancelPreviousAndExecuteOnlyLast = new() {
		Sequentially = true, CancelPrevious = true, ExecuteOnlyLast = true
	};
	public string Name { get; set; }
	public bool ByInstance { get; set; }
	public bool Sequentially { get; set; }
	public bool ExecuteOnlyLast { get; set; }
	public bool CancelPrevious { get; set; }
}