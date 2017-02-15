using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class SessionPreview
    {
        internal SessionPreview() { }
        internal SessionPreview(Guid id) { Id = id; }
        public Guid Id { get; set; }
        public IList<WorkPreview> Works { get; set; }
        public void Print() => Printer.Foreach("Preview: ", Works, work => work.Print());
    }
}
