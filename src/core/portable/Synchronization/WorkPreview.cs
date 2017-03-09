using Fuxion.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    [DataContract(IsReference = true)]
    public class WorkPreview
    {
        internal WorkPreview() { }
        internal WorkPreview(Guid id)
        {
            Id = id;
        }
        [DataMember]
        public SessionPreview Session { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string MasterSideName { get; set; }
        [DataMember]
        public IList<ItemPreview> Items { get; set; }
        public int ChangesCount { get { return Items.Sum(i => i.ChangesCount); } }
        public string ChangesMessage
        {
            get
            {
                var count = ChangesCount;
                return $"{count} {(count == 1 ? Strings.Change.ToLower() : Strings.Changes.ToLower())}";
            }
        }
        public void Print() => Printer.Foreach("Work:", Items, item => item.Print(), false);
    }
}
