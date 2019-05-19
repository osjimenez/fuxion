using Fuxion.Resources;
using System;
using System.Resources;
using System.Runtime.Serialization;

namespace Fuxion.Synchronization
{
	[DataContract(IsReference = true)]
	public class PropertyPreview
	{
#nullable disable
		internal PropertyPreview() { }
#nullable enable
		internal PropertyPreview(ItemSidePreview itemSide) : this()
		{
			ItemSide = itemSide;
		}
		internal PropertyPreview(ItemRelationPreview relation) : this()
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
				ResourceManager? resman = null;
				if (ItemSide != null)
					resman = ItemSide.Item.Work.Session.ResourceManager;
				else if (Relation?.ItemSide != null)
				{
					ResourceManager? SearchResourceManager(ItemRelationPreview rel)
					{
						if (rel.ItemSide != null)
							return rel.ItemSide.Item.Work.Session.ResourceManager;
						return SearchResourceManager(rel.Relation);
					}
					resman = SearchResourceManager(Relation);
				}
				return string.Format(Strings.PropertyWillBeChanged,
						resman != null ? (resman.GetString(PropertyName) ?? PropertyName) : PropertyName,
						SideValue,
						MasterValue);
			}
		}
	}
}
