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
    public class Side<TSource, TItem> : ISide
    {
        public bool IsMaster { get; set; }
        public string Name { get; set; }
        public string SingularItemTypeName { get; set; }
        public string PluralItemTypeName { get; set; }
        public bool ItemTypeIsMale { get; set; }

        public TSource Source { get; set; }
        public Func<TItem, string> OnNaming { get; set; }
        public Func<TSource, ICollection<TItem>> OnLoad { get; set; }
        public Action<TSource, TItem> OnInsert { get; set; }
        public Action<TSource, TItem> OnDelete { get; set; }
        public Action<TSource, TItem> OnUpdate { get; set; }

        internal Side<TSource, TItem> Clone()
        {
            return new Side<TSource, TItem>
            {
                IsMaster = IsMaster,
                Name = Name,
                SingularItemTypeName = SingularItemTypeName,
                PluralItemTypeName = PluralItemTypeName,
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
