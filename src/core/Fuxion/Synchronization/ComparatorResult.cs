using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fuxion.Synchronization
{
    internal class ComparatorResult<TItemA, TItemB, TKey> : IComparatorResultInternal
    {
        public TKey Key { get; set; }
        object IComparatorResultInternal.Key { get { return Key; } }
        public TItemA MasterItem { get; set; }
        object IComparatorResultInternal.MasterItem { get { return MasterItem; } }
        public TItemB SideItem { get; set; }
        object IComparatorResultInternal.SideItem { get { return SideItem; } }
        public ICollection<IPropertyRunner> Properties { get; set; } = new List<IPropertyRunner>();
        public ICollection<ISideRunner> SubSides { get; set; } = new List<ISideRunner>();
        //public void AddProperty<TPropertyA, TPropertyB>(string propertyName, TPropertyA aValue, TPropertyB bValue)
        //{
        //    Properties.Add(new PropertyRunner<TPropertyA, TPropertyB>(propertyName, aValue, bValue));
        //}
        //public void AddProperty<TPropertyA, TPropertyB>(Expression<Func<TPropertyA>> aProperty, Expression<Func<TPropertyB>> bProperty)
        //{
        //    Properties.Add(new PropertyRunner<TPropertyA, TPropertyB>(aProperty.GetMemberName(), aProperty.Compile().Invoke(), bProperty.Compile().Invoke()));
        //}
    }
}