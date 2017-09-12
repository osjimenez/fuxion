using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
    public interface IKeyValueEntry<TKey, TValue>
    {
        TKey Key { get; set; }
        TValue Value { get; set; }
    }
}
