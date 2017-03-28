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
    public class ItemPreview
    {
        internal ItemPreview() { }
        internal ItemPreview(WorkPreview work, Guid id)
        {
            Work = work;
            Id = id;
        }
        [DataMember]
        public WorkPreview Work { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public bool MasterItemExist { get; set; }
        [DataMember]
        public string MasterItemName { get; set; }
        [DataMember]
        public string SingularMasterTypeName { get; set; }
        [DataMember]
        public string PluralMasterTypeName { get; set; }
        [DataMember]
        public bool MasterTypeIsMale { get; set; }
        [DataMember]
        public IList<ItemSidePreview> Sides { get; set; }
        public int ChangesCount { get { return Sides.Sum(s => s.ChangesCount); } }
        public string ChangesMessage
        {
            get
            {
                var count = ChangesCount;
                return $"{count} {(count == 1 ? Strings.Change.ToLower() : Strings.Changes.ToLower())}";
            }
        }
        public void Print(IPrinter printer)
        {
            printer.Foreach($"Item '{(MasterItemExist ? MasterItemName : "null")}' has '{Sides.Count()}' sides", Sides, side =>
            {
                Action<ICollection<ItemRelationPreview>> printRelations = null;
                printRelations = rels =>
                {
                    if (!rels.Any()) return;
                    printer.Foreach("Relations: ", rels, rel =>
                    {
                        printer.Foreach($"{rel.Action.ToString().ToUpper()} - In '{rel.SideName}' side is named '{rel.SideItemName}' with key '{rel.Key}' and has '{rel.Properties.Count()}' change(s)", rel.Properties, pro2 =>
                        {
                            printer.WriteLine($"Property '{pro2.PropertyName}' will be changed from '{pro2.SideValue}' to '{pro2.MasterValue}'");
                        });
                        printRelations(rel.Relations);
                    });
                };

                if (side.SideItemExist)
                {
                    printer.Foreach($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side is named '{side.SideItemName}' with key '{side.Key}' and has '{side.Properties.Count()}' change(s)", side.Properties, pro =>
                    {
                        printer.WriteLine($"Property '{pro.PropertyName}' will be changed from '{pro.SideValue}' to '{pro.MasterValue}'");
                    });
                    printRelations(side.Relations);
                }
                else
                {
                    printer.WriteLine($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side because does not exist");
                    printRelations(side.Relations);
                }
            }, false);
        }

    }
}
