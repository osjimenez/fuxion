namespace Fuxion.Application.Snapshots;

public interface ISnapshottable
{
	Snapshot TakeSnapshot();
	void     ApplySnapshot(Snapshot snapshot);
}