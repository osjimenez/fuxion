using Fuxion.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
	public class MemoryCachedKeyValueRepository<TKey, TValue> : IKeyValueRepository<TKey, TValue>
	{
		public MemoryCachedKeyValueRepository(IKeyValueRepository<TKey, TValue> originRepository)
		{
			_origin = originRepository;
		}
		IKeyValueRepository<TKey, TValue> _origin;
		Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>> dic = new Locker<Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>>(new Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>());

		public bool Exist(TKey key)
			=> dic.Write(d =>
			{
				if (d.ContainsKey(key)) return d[key].HasOrigin;
				var res = new MemoryKeyValueRepositoryValue<TValue>();
				try
				{
					res.Value = _origin.Get(key);
					res.HasOrigin = true;
				}
				catch
				{
					res.Value = default(TValue);
					res.HasOrigin = false;
				}
				d.Add(key, res);
				return res.HasOrigin;
			});
		public TValue Find(TKey key)
			=> dic.Write(d =>
			{
				if (key == null) return default(TValue);
				if (d.ContainsKey(key)) return d[key].Value;
				var res = new MemoryKeyValueRepositoryValue<TValue>();
				try
				{
					res.Value = _origin.Get(key);
					res.HasOrigin = true;
				}
				catch
				{
					res.Value = default(TValue);
					res.HasOrigin = false;
				}
				d.Add(key, res);
				return res.Value;
			});
		public TValue Get(TKey key)
			=> dic.Write(d =>
			{
				if (d.ContainsKey(key)) return d[key].Value;
				var res = new MemoryKeyValueRepositoryValue<TValue>();
				try
				{
					res.Value = _origin.Get(key);
					res.HasOrigin = true;
					return res.Value;
				}
				catch
				{
					res.Value = default(TValue);
					res.HasOrigin = false;
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
				else d[key] = new MemoryKeyValueRepositoryValue<TValue>
				{
					HasOrigin = true,
					Value = value
				};
				_origin.Set(key, value);
			});
	}
	public class MemoryCachedAsyncKeyValueRepository<TKey, TValue> : IAsyncKeyValueRepository<TKey, TValue>
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
				var res = new MemoryKeyValueRepositoryValue<TValue>();
				try
				{
					res.Value = await _origin.GetAsync(key);
					res.HasOrigin = true;
				}
				catch
				{
					res.Value = default(TValue);
					res.HasOrigin = false;
				}
				d.Add(key, res);
				return res.HasOrigin;
			});
		public Task<TValue> FindAsync(TKey key)
			=> dic.WriteAsync(async d =>
			{
				if (d.ContainsKey(key)) return d[key].Value;
				var res = new MemoryKeyValueRepositoryValue<TValue>();
				try
				{
					res.Value = await _origin.GetAsync(key);
					res.HasOrigin = true;
				}
				catch
				{
					res.Value = default(TValue);
					res.HasOrigin = false;
				}
				d.Add(key, res);
				return res.Value;
			});
		public Task<TValue> GetAsync(TKey key)
			=> dic.WriteAsync(async d =>
			{
				if (d.ContainsKey(key)) return d[key].Value;
				var res = new MemoryKeyValueRepositoryValue<TValue>();
				try
				{
					res.Value = await _origin.GetAsync(key);
					res.HasOrigin = true;
					return res.Value;
				}
				catch
				{
					res.Value = default(TValue);
					res.HasOrigin = false;
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
				else d[key] = new MemoryKeyValueRepositoryValue<TValue>
				{
					HasOrigin = true,
					Value = value
				};
				return _origin.SetAsync(key, value);
			});
	}
	class MemoryKeyValueRepositoryValue<TValue>
    {
        public bool HasOrigin { get; set; }
        public TValue Value { get; set; }
    }
}
