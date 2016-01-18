using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
    public interface IKeyValueRepository<TKeyValueEntry, TKey, TValue>
        where TKeyValueEntry : IKeyValueEntry<TKey,TValue>
    {
        Task<bool> ExistAsync(TKey key);
        Task<TValue> FindAsync(TKey key);
        Task<TValue> GetAsync(TKey key);
        Task SetAsync(TKey key, TValue value);
        Task RemoveAsync(TKey key);
    }
}
