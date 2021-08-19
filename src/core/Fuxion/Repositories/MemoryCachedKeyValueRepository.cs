namespace Fuxion.Repositories;

using Fuxion.Threading;

public class MemoryCachedKeyValueRepository<TKey, TValue> : IKeyValueRepository<TKey, TValue> where TKey : notnull
{
	public MemoryCachedKeyValueRepository(IKeyValueRepository<TKey, TValue> originRepository) => _origin = originRepository;

	private readonly IKeyValueRepository<TKey, TValue> _origin;
	private readonly Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>> dic = new Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>>(new Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>());

	public bool Exist(TKey key)
		=> dic.Write(d =>
		{
			if (d.ContainsKey(key)) return d[key].HasOrigin;
			MemoryKeyValueRepositoryValue<TValue> res = default!;
			try
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(true, _origin.Get(key));
			}
			catch
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(false, default!);
			}
			d.Add(key, res);
			return res.HasOrigin;
		});
	public TValue Find(TKey key)
		=> dic.Write(d =>
		{
			if (key == null) return default!;
			if (d.ContainsKey(key)) return d[key].Value;
			MemoryKeyValueRepositoryValue<TValue> res = default!;
			try
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(true, _origin.Get(key));
			}
			catch
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(false, default!);
			}
			d.Add(key, res);
			return res.Value;
		});
	public TValue Get(TKey key)
		=> dic.Write(d =>
		{
			if (d.ContainsKey(key)) return d[key].Value;
			MemoryKeyValueRepositoryValue<TValue> res = default!;
			try
			{
				res = new MemoryKeyValueRepositoryValue<TValue>(true, _origin.Get(key));
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
	public void Remove(TKey key)
		=> dic.Write(d =>
		{
			d.Remove(key);
			_origin.Remove(key);
		});
	public void Set(TKey key, TValue value)
		=> dic.Write(d =>
		{
			if (d.ContainsKey(key)) d[key].Value = value;
			else d[key] = new MemoryKeyValueRepositoryValue<TValue>(true, value);
			_origin.Set(key, value);
		});
}