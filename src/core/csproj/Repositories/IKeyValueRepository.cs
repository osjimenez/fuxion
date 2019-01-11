using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
    public interface IKeyValueRepository<TKey, TValue>
    {
        bool Exist(TKey key);
        TValue Find(TKey key);
        TValue Get(TKey key);
        void Set(TKey key, TValue value);
        void Remove(TKey key);
    }
	public interface IAsyncKeyValueRepository<TKey, TValue>
	{
		Task<bool> ExistAsync(TKey key);
		Task<TValue> FindAsync(TKey key);
		Task<TValue> GetAsync(TKey key);
		Task SetAsync(TKey key, TValue value);
		Task RemoveAsync(TKey key);
	}
}
