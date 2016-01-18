using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
    public class MemoryKeyValueRepository<TKeyValueEntry, TKey, TValue> : IKeyValueRepository<TKeyValueEntry, TKey, TValue>
        where TKeyValueEntry : IKeyValueEntry<TKey, TValue>
    {
        public MemoryKeyValueRepository(IKeyValueRepository<TKeyValueEntry, TKey, TValue> originRepository)
        {
            _origin = originRepository;
        }
        IKeyValueRepository<TKeyValueEntry, TKey, TValue> _origin;
        Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>> dic = new Dictionary<TKey, MemoryKeyValueRepositoryValue<TValue>>();
        public async Task<bool> ExistAsync(TKey key)
        {
            if (dic.ContainsKey(key)) return dic[key].HasOrigin;
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
            dic.Add(key, res);
            return res.HasOrigin;
        }
        public async Task<TValue> FindAsync(TKey key)
        {
            if (dic.ContainsKey(key)) return dic[key].Value;
            var res = new MemoryKeyValueRepositoryValue<TValue>();
            try {
                res.Value = await _origin.GetAsync(key);
                res.HasOrigin = true;
            }
            catch {
                res.Value = default(TValue);
                res.HasOrigin = false;
            }
            dic.Add(key, res);
            return res.Value;
        }
        public async Task<TValue> GetAsync(TKey key)
        {
            if (dic.ContainsKey(key)) return dic[key].Value;
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
                dic.Add(key, res);
            }
        }
        public Task RemoveAsync(TKey key)
        {
            dic.Remove(key);
            return _origin.RemoveAsync(key);
        }
        public Task SetAsync(TKey key, TValue value)
        {
            if (dic.ContainsKey(key)) dic[key].Value = value;
            else dic[key] = new MemoryKeyValueRepositoryValue<TValue>
            {
                HasOrigin = true,
                Value = value
            };
            return _origin.SetAsync(key, value);
        }
    }
    class MemoryKeyValueRepositoryValue<TValue>
    {
        public bool HasOrigin { get; set; }
        public TValue Value { get; set; }
    }
}
