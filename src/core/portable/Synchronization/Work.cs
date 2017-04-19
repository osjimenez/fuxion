using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Fuxion.Synchronization
{
    public class Work
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool LoadSidesInParallel { get; set; } = true;
        public IEnumerable<ISide> Sides { get; set; }
        public IEnumerable<IComparator> Comparators { get; set; }
        public Action<SessionPreview> PostPreviewAction { get; set; }
    }
}