using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class Session
    {
        internal Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool MakePreviewInParallel { get; set; } = true;
        public ICollection<Work> Works { get; set; } = new List<Work>();
    }
}
