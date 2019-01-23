using Fuxion.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    [DataContract(IsReference = true)]
    public class PropertyPreview
    {
        internal PropertyPreview() { }
        internal PropertyPreview(ItemSidePreview itemSide) {
            ItemSide = itemSide;
        }
        internal PropertyPreview(ItemRelationPreview relation)
        {
            Relation = relation;
        }
        [DataMember]
        public ItemSidePreview ItemSide { get; set; }
        [DataMember]
        public ItemRelationPreview Relation { get; set; }
        [DataMember]
        public string PropertyName { get; set; }
        [DataMember]
        public string MasterValue { get; set; }
        [DataMember]
        public string SideValue { get; set; }

        public string StatusMessage
        {
            get
            {
                ResourceManager resman = null;
                if (ItemSide != null)
                    resman = ItemSide.Item.Work.Session.ResourceManager;
                else if (Relation?.ItemSide != null)
                {
                    Func<ItemRelationPreview, ResourceManager> searchResourceManager = null;
                    searchResourceManager = new Func<ItemRelationPreview, ResourceManager>(rel =>
                    {
                        if (rel.ItemSide != null)
                            return rel.ItemSide.Item.Work.Session.ResourceManager;
                        return searchResourceManager(rel.Relation);
                    });
                    resman = searchResourceManager(Relation);
                }
                return string.Format(Strings.PropertyWillBeChanged,
                        resman != null ? (resman.GetString(PropertyName) ?? PropertyName) : PropertyName,
                        SideValue,
                        MasterValue);
            }
        }
    }
}
