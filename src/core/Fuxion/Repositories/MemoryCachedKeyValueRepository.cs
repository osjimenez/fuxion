using Fuxion.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
	public class MemoryCachedKeyValueRepository<TKey, TValue> : IKeyValueRepository<TKey, TValue> where TKey : notnull
	{
		public MemoryCachedKeyValueRepository(IKeyValueRepository<TKey, TValue> originRepository)
		{
			_origin = originRepository;
		}

		readonly IKeyValueRepository<TKey, TValue> _origin;
		readonly Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>> dic = new Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>>(new Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>());

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
	public class MemoryCachedAsyncKeyValueRepository<TKey, TValue> : IAsyncKeyValueRepository<TKey, TValue> where TKey : notnull
	{
		public MemoryCachedAsyncKeyValueRepository(IAsyncKeyValueRepository<TKey, TValue> originRepository)
		{
			_origin = originRepository;
		}
		IAsyncKeyValueRepository<TKey, TValue> _origin;
		Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>> dic = new Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>>(new Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>());

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
	class MemoryKeyValueRepositoryValue<TValue>
	{
		public MemoryKeyValueRepositoryValue(bool hasOrigin, TValue value)
		{
			HasOrigin = hasOrigin;
			Value = value;
		}
		public bool HasOrigin { get; set; }
		public TValue Value { get; set; }
	}
}
