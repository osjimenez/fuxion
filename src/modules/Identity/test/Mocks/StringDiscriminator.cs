using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class StringDiscriminator : IDiscriminator<string, string>
    {
        public StringDiscriminator(string id, IEnumerable<string> inclusions, IEnumerable<string> exclusions, string typeId)
        {
            Id = id;
            Inclusions = inclusions;
            Exclusions = exclusions;
            TypeId = typeId;
        }
        public string Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }
        public string Name { get { return Id; } }
        IEnumerable<object> IDiscriminator.Inclusions { get { return ((IDiscriminator<string, string>)this).Inclusions.Cast<object>(); } }
        public IEnumerable<string> Inclusions { get; set; }
        IEnumerable<object> IDiscriminator.Exclusions { get { return ((IDiscriminator<string, string>)this).Exclusions.Cast<object>(); } }
        public IEnumerable<string> Exclusions { get; set; }
        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get { return TypeId; } }
    }
}
