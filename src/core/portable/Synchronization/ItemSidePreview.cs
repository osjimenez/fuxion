using Fuxion.ComponentModel;
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
    public class ItemSidePreview : Notifier<ItemSidePreview>
    {
        internal ItemSidePreview() {}
        internal ItemSidePreview(ItemPreview item, Guid id) : this()
        {
            Item = item;
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
                        isReadOnly = !IsEnabled;
                        break;
                    case SynchronizationAction.Delete:
                        // Activado => Todos los elementos contenidos se activarán y no serán editables
                        // Desactivado => Todos los elementos contenidos se desactivarán y serán editables
                        isEnabled = IsEnabled;
                        isReadOnly = IsEnabled;
                        break;
                    case SynchronizationAction.Update:
                        // Activado => Todos los elementos contenidos se activarán y serán editables
                        // Desactivado => Todos los elementos contenidos se desactivarán y serán editables
                        isEnabled = IsEnabled;
                        isReadOnly = false;
                        break;
                    case SynchronizationAction.None:
                        if(Relations.Any(r=>r.Action != SynchronizationAction.None))
                        {
                            isEnabled = IsEnabled;
                            isReadOnly = false;
                        }
                        break;
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
        public ItemPreview Item { get { return GetValue<ItemPreview>(); } set { SetValue(value); } }
        [DataMember]
        public Guid Id { get { return GetValue<Guid>(); } set { SetValue(value); } }

        [DataMember]
        public bool SideAllowDelete { get { return GetValue<bool>(); } set { SetValue(value); } }
        [DataMember]
        public bool SideAllowInsert { get { return GetValue<bool>(); } set { SetValue(value); } }
        [DataMember]
        public bool SideAllowUpdate { get { return GetValue<bool>(); } set { SetValue(value); } }

        [DataMember]
        public bool SideItemExist { get { return GetValue<bool>(); } set { SetValue(value); } }
        [DataMember]
        public string SideName { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public string SingularSideTypeName { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public string PluralSideTypeName { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public bool ItemTypeIsMale { get { return GetValue<bool>(); } set { SetValue(value); } }
        [DataMember]
        public string Key { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public string SideItemName { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public string SideItemTag { get { return GetValue<string>(); } set { SetValue(value); } }
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
        // Messages:
        // ChangesCount   - 3
        // ChangesMessage - 3 cambios
        // StatusMessage  - La entidad de tipo 'Usuario' llamada 'Oscar Jimenez Sainz' será agregada a 'Salto'
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
                        return string.Format(Strings.EntityWillBeIgnored,
                            SingularSideTypeName,
                            SideItemName,
                            SideName);
                    case SynchronizationAction.Insert:
                        return IsEnabled
                            ? string.Format(Strings.EntityWillBeAdded,
                                SingularSideTypeName,
                                Item.MasterItemName,
                                SideName)
                            : string.Format(Strings.EntityWillBeIgnored,
                                SingularSideTypeName,
                                SideItemName,
                                SideName);
                    case SynchronizationAction.Delete:
                        return IsEnabled
                            ? string.Format(Strings.EntityWillBeDeleted,
                                SingularSideTypeName,
                                SideItemName,
                                SideName)
                            : string.Format(Strings.EntityWillBeIgnored,
                                SingularSideTypeName,
                                SideItemName,
                                SideName);
                    case SynchronizationAction.Update:
                        return IsEnabled
                            ? string.Format(Strings.EntityWillBeUpdated,
                                SingularSideTypeName,
                                SideItemName,
                                SideName)
                            : string.Format(Strings.EntityWillBeIgnored,
                                SingularSideTypeName,
                                SideItemName,
                                SideName);
                    default:
                        return string.Format(Strings.EntityWillBeIgnored,
                            SingularSideTypeName,
                            SideItemName,
                            SideName);
                }
            }
        }
        //public string[] DetailedMessages
        //{
        //    get
        //    {
        //        var res = new List<string>();
        //        if (!Item.MasterItemExist)
        //            res.Add(string.Format(Strings.EntityWillBeDeleted,
        //                SingularSideTypeName,
        //                SideItemName,
        //                SideName));
        //        if (!SideItemExist)
        //            res.Add(string.Format(Strings.EntityWillBeAdded,
        //                SingularSideTypeName,
        //                Item.MasterItemName,
        //                SideName));
        //        foreach (var pro in Properties)
        //            res.Add(pro.StatusMessage);
        //        Func<ICollection<ItemRelationPreview>, string[]> processRelations = null;
        //        processRelations = new Func<ICollection<ItemRelationPreview>, string[]>(rels =>
        //        {
        //            var resRelations = new List<string>();
        //            foreach (var rel in rels)
        //            {
        //                switch (rel.Action)
        //                {
        //                    case SynchronizationAction.None:
        //                        break;
        //                    case SynchronizationAction.Insert:
        //                        resRelations.Add(string.Format(Strings.RelationWillBeAdded,
        //                            rel.SingularMasterTypeName,
        //                            rel.MasterItemName,
        //                            rel.SideName));
        //                        break;
        //                    case SynchronizationAction.Delete:
        //                        resRelations.Add(string.Format(Strings.RelationWillBeDeleted,
        //                            rel.SingularSideTypeName,
        //                            rel.SideItemName,
        //                            rel.SideName));
        //                        break;
        //                    case SynchronizationAction.Update:
        //                        foreach (var pro in rel.Properties)
        //                        {
        //                            resRelations.Add(string.Format(Strings.RelationWillBeChanged,
        //                                pro.PropertyName,
        //                                rel.SideItemName,
        //                                pro.SideValue,
        //                                pro.MasterValue));
        //                        }
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                resRelations.AddRange(processRelations(rel.Relations));
        //            }
        //            return resRelations.ToArray();
        //        });
        //        res.AddRange(processRelations(Relations));
        //        return res.ToArray();
        //    }
        //}
    }
}
