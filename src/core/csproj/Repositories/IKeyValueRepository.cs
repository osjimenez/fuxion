using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
    public interface IKeyValueRepository<TKey, TValue>
    {
        Task<bool> ExistAsync(TKey key);
        bool Exist(TKey key);

        Task<TValue> FindAsync(TKey key);
        TValue Find(TKey key);

        Task<TValue> GetAsync(TKey key);
        TValue Get(TKey key);

        Task SetAsync(TKey key, TValue value);
        void Set(TKey key, TValue value);

        Task RemoveAsync(TKey key);
        void Remove(TKey key);
    }
}
