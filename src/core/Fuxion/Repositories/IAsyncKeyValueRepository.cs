namespace Fuxion.Repositories;

public interface IAsyncKeyValueRepository<TKey, TValue>
{
	Task<bool> ExistAsync(TKey key);
	Task<TValue?> FindAsync(TKey key);
	Task<TValue> GetAsync(TKey key);
	Task SetAsync(TKey key, TValue value);
	Task RemoveAsync(TKey key);
}