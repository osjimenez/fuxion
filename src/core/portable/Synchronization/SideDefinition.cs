using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SideDefinition<TSource, TItem, TKey> : ISideDefinition
    {
        public bool IsMaster { get; set; }
        public string Name { get; set; }
        //public string SingularItemTypeName { get; set; }
        //public string PluralItemTypeName { get; set; }

        public TSource Source { get; set; }
        public Func<TItem, string> OnNaming { get; set; }
        public Func<TSource, ICollection<TItem>> OnLoad { get; set; }
        public Action<TSource, TItem> OnInsert { get; set; }
        public Action<TSource, TItem> OnDelete { get; set; }
        public Action<TSource, TItem> OnUpdate { get; set; }

        internal SideDefinition<TSource, TItem, TKey> Clone()
        {
            return new SideDefinition<TSource, TItem, TKey>
            {
                IsMaster = IsMaster,
                Name = Name,
                //SingularItemTypeName = SingularItemTypeName,
                //PluralItemTypeName = PluralItemTypeName,
                Source = Source,
                OnNaming = OnNaming,
                OnLoad = OnLoad,
                OnInsert = OnInsert,
                OnDelete = OnDelete,
                OnUpdate = OnUpdate
            };
        }
    }
}
