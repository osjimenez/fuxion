using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class ItemPreview
    {
        internal ItemPreview() { }
        internal ItemPreview(Guid id) { Id = id; }
        public Guid Id { get; set; }
        public bool MasterItemExist { get; set; }
        public string MasterItemName { get; set; }
        public IList<ItemSidePreview> Sides { get; set; }
        public void Print()
        {
            Printer.Foreach($"Item '{(MasterItemExist ? MasterItemName : "null")}' has '{Sides.Count()}' sides", Sides, side =>
            {
                Action<ICollection<ItemRelationPreview>> printRelations = null;
                printRelations = rels =>
                {
                    if (!rels.Any()) return;
                    Printer.Foreach("Relations: ", rels, rel =>
                    {
                        //Printer.WriteLine($"Relation '{(rel.MasterItemExist ? rel.MasterItemName : "null")}' has '{rel.Relations.Count()}' relations");
                        Printer.Foreach($"{rel.Action.ToString().ToUpper()} - In '{rel.SideName}' side is named '{rel.SideItemName}' with key '{rel.Key}' and has '{rel.Properties.Count()}' change(s)", rel.Properties, pro2 =>
                        {
                            Printer.WriteLine($"Property '{pro2.PropertyName}' will be changed from '{pro2.SideValue}' to '{pro2.MasterValue}'");
                        });
                        printRelations(rel.Relations);
                    });
                };

                if (side.SideItemExist)
                {
                    Printer.Foreach($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side is named '{side.SideItemName}' with key '{side.Key}' and has '{side.Properties.Count()}' change(s)", side.Properties, pro =>
                    {
                        Printer.WriteLine($"Property '{pro.PropertyName}' will be changed from '{pro.SideValue}' to '{pro.MasterValue}'");
                    });
                    printRelations(side.Relations);
                }
                else
                {
                    Printer.WriteLine($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side does not exist");
                    printRelations(side.Relations);
                }
            });
        }
    }
}
