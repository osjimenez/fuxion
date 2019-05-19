using Fuxion.ComponentModel;
using Fuxion.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Fuxion.Synchronization
{
	[DataContract(IsReference = true)]
	public class ItemRelationPreview : Notifier<ItemRelationPreview>
	{
#nullable disable
		internal ItemRelationPreview() { }
#nullable enable
		internal ItemRelationPreview(ItemSidePreview itemSide, Guid id) : this()
		{
			ItemSide = itemSide;
			Id = id;
		}
		internal ItemRelationPreview(ItemRelationPreview relation, Guid id) : this()
		{
			Relation = relation;
			Id = id;
		}
		protected override void OnInitialize()
		{
			PropertyChanged += (s, e) => e.Case(() => IsEnabled, a =>
			{
				bool isEnabled = false;
				bool isReadOnly = false;
				switch (Action)
				{
					case SynchronizationAction.Insert:
						// Activado => Todos los elementos contenidos se activarán y serán editables
						// Desactivado => Todos los elementos contenidos se desactivarán y no serán editables
						isEnabled = IsEnabled;
						isReadOnly = IsEnabled;
						break;
					case SynchronizationAction.Delete:
						// Activado => Todos los elementos contenidos se activarán y no serán editables
						// Desactivado => Todos los elementos contenidos se desactivarán y serán editables
						isEnabled = IsEnabled;
						isReadOnly = !IsEnabled;
						break;
					case SynchronizationAction.Update:
						// Activado => Todos los elementos contenidos se activarán y serán editables
						// Desactivado => Todos los elementos contenidos se desactivarán y serán editables
						isEnabled = IsEnabled;
						isReadOnly = false;
						break;
					case SynchronizationAction.None:
					default:
						break;

				}
				foreach (var rel in Relations)
				{
					rel.IsEnabled = isEnabled;
					rel.IsReadOnly = isReadOnly;
				}
			});
		}
		[DataMember]
		public ItemSidePreview ItemSide { get; set; }
		[DataMember]
		public ItemRelationPreview Relation { get; set; }

		[DataMember]
		public bool SideAllowDelete { get { return GetValue<bool>(); } set { SetValue(value); } }
		[DataMember]
		public bool SideAllowInsert { get { return GetValue<bool>(); } set { SetValue(value); } }
		[DataMember]
		public bool SideAllowUpdate { get { return GetValue<bool>(); } set { SetValue(value); } }

		[DataMember]
		public Guid Id { get; set; }
		[DataMember]
		public bool MasterItemExist { get; set; }
		[DataMember]
		public string MasterItemName { get; set; }
		[DataMember]
		public string MasterItemTag { get; set; }
		[DataMember]
		public string SingularMasterTypeName { get; set; }
		[DataMember]
		public string PluralMasterTypeName { get; set; }
		[DataMember]
		public bool SideItemExist { get; set; }
		[DataMember]
		public string SideName { get; set; }
		[DataMember]
		public string SingularSideTypeName { get; set; }
		[DataMember]
		public string PluralSideTypeName { get; set; }
		[DataMember]
		public string Key { get; set; }
		[DataMember]
		public string SideItemName { get; set; }
		[DataMember]
		public string SideItemTag { get; set; }
		[DataMember]
		public bool IsReadOnly { get { return GetValue<bool>(); } set { SetValue(value); } }
		[DataMember]
		public bool IsEnabled { get { return GetValue(() => true); } set { SetValue(value); RaisePropertyChanged(() => StatusMessage, "", ""); } }
		[DataMember]
		public SynchronizationAction Action { get { return GetValue<SynchronizationAction>(); } set { SetValue(value); } }
		[DataMember]
		public ICollection<PropertyPreview> Properties { get { return GetValue<ICollection<PropertyPreview>>(() => new List<PropertyPreview>()); } set { SetValue(value); } }
		[DataMember]
		public ICollection<ItemRelationPreview> Relations { get { return GetValue<ICollection<ItemRelationPreview>>(() => new List<ItemRelationPreview>()); } set { SetValue(value); } }

		public int ChangesCount
		{
			get
			{
				return (Action != SynchronizationAction.None ? 1 : 0) + Properties.Count + Relations.Sum(r => r.ChangesCount);
			}
		}
		public string ChangesMessage
		{
			get
			{
				var count = ChangesCount;
				return $"{count} {(count == 1 ? Strings.Change.ToLower() : Strings.Changes.ToLower())}";
			}
		}
		public string StatusMessage
		{
			get
			{
				switch (Action)
				{
					case SynchronizationAction.None:
						return string.Format(Strings.RelationWillBeIgnored,
							SingularSideTypeName,
							(string.IsNullOrWhiteSpace(MasterItemName) ? SideItemName : MasterItemName),
							SideName);
					case SynchronizationAction.Insert:
						return IsEnabled
							? string.Format(Strings.RelationWillBeAdded,
								SingularSideTypeName,
								MasterItemName,
								SideName)
							: string.Format(Strings.RelationWillBeIgnored,
								SingularSideTypeName,
								(string.IsNullOrWhiteSpace(MasterItemName) ? SideItemName : MasterItemName),
								SideName);
					case SynchronizationAction.Delete:
						return IsEnabled
							? string.Format(Strings.RelationWillBeDeleted,
								SingularSideTypeName,
								SideItemName,
								SideName)
							: string.Format(Strings.RelationWillBeIgnored,
								SingularSideTypeName,
								(string.IsNullOrWhiteSpace(MasterItemName) ? SideItemName : MasterItemName),
								SideName);
					case SynchronizationAction.Update:
						return IsEnabled
							? string.Format(Strings.RelationWillBeUpdated,
								SingularSideTypeName,
								SideItemName,
								SideName)
							: string.Format(Strings.RelationWillBeIgnored,
								SingularSideTypeName,
								(string.IsNullOrWhiteSpace(MasterItemName) ? SideItemName : MasterItemName),
								SideName);
					default:
						return string.Format(Strings.RelationWillBeIgnored,
							SingularSideTypeName,
							(string.IsNullOrWhiteSpace(MasterItemName) ? SideItemName : MasterItemName),
							SideName);
				}
			}
		}
	}
}
