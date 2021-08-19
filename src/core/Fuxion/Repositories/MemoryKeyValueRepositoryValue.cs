namespace Fuxion.Repositories;

internal class MemoryKeyValueRepositoryValue<TValue>
{
	public MemoryKeyValueRepositoryValue(bool hasOrigin, TValue value)
	{
		HasOrigin = hasOrigin;
		Value = value;
	}
	public bool HasOrigin { get; set; }
	public TValue Value { get; set; }
}