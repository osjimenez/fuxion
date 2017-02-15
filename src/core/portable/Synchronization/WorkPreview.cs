using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class WorkPreview
    {
        internal WorkPreview() { }
        internal WorkPreview(Guid id) { Id = id; }
        public Guid Id { get; set; }
        public IList<ItemPreview> Items { get; set; }
        public void Print() => Printer.Foreach("Work:", Items, item => item.Print());
    }
}
