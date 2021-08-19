namespace Fuxion.Repositories;

using Fuxion.Threading;

public class MemoryCachedAsyncKeyValueRepository<TKey, TValue> : IAsyncKeyValueRepository<TKey, TValue> where TKey : notnull
{
	public MemoryCachedAsyncKeyValueRepository(IAsyncKeyValueRepository<TKey, TValue> originRepository) => _origin = originRepository;

	private readonly IAsyncKeyValueRepository<TKey, TValue> _origin;
	private readonly Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>> dic = new Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>>(new Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>());

	public Task<bool> ExistAsync(TKey key)
		=> dic.WriteAsync(async d =>
		{
			if (d.ContainsKey(key)) return d[key].HasOrigin;
			MemoryKeyValueRepositoryValue<TValue> res = default!;
			try
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(true, await _origin.GetAsync(key));
			}
			catch
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(false, default!);
			}
			d.Add(key, res);
			return res.HasOrigin;
		});
	public Task<TValue?> FindAsync(TKey key)
		=> dic.WriteAsync(async d =>
		{
			if (key == null) return default!;
			if (d.ContainsKey(key)) return d[key].Value;
			MemoryKeyValueRepositoryValue<TValue> res = default!;
			try
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(true, await _origin.GetAsync(key));
			}
			catch
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(false, default!);
			}
			d.Add(key, res);
			return (TValue?)res.Value;
		});
	public Task<TValue> GetAsync(TKey key)
		=> dic.WriteAsync(async d =>
		{
			if (d.ContainsKey(key)) return d[key].Value;
			MemoryKeyValueRepositoryValue<TValue> res = default!;
			try
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(true, await _origin.GetAsync(key));
				return res.Value;
			}
			catch
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(false, default!);
				throw;
			}
			finally
			{
				d.Add(key, res);
			}
		});
	public Task RemoveAsync(TKey key)
		=> dic.WriteAsync(d =>
		{
			d.Remove(key);
			return _origin.RemoveAsync(key);
		});
	public Task SetAsync(TKey key, TValue value)
		=> dic.WriteAsync(d =>
		{
			if (d.ContainsKey(key)) d[key].Value = value;
			else d[key] = new MemoryKeyValueRepositoryValue<TValue>(true, value);
			return _origin.SetAsync(key, value);
		});
}