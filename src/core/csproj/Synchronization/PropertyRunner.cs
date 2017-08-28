using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class PropertyRunner<TMaster, TSide, TMasterProperty, TSideProperty> : IPropertyRunner
    {
        public PropertyRunner(string propertyName, TMasterProperty masterValue, TSideProperty sideValue, Func<TMasterProperty, string> masterNamingFunction, Func<TSideProperty, string> sideNamingFunction)
        {
            PropertyName = propertyName;
            MasterValue = masterValue;
            SideValue = sideValue;
            MasterNamingFunction = masterNamingFunction ?? (_ => _?.ToString());
            SideNamingFunction = sideNamingFunction ?? (_ => _?.ToString());
        }
        public string PropertyName { get; set; }
        public TMasterProperty MasterValue { get; }
        object IPropertyRunner.MasterValue { get { return MasterValue; } }
        public TSideProperty SideValue { get; }
        object IPropertyRunner.SideValue { get { return SideValue; } }
        public Func<TMasterProperty, string> MasterNamingFunction { get; set; }
        public Func<TSideProperty, string> SideNamingFunction { get; set; }
        Func<object, string> IPropertyRunner.MasterNamingFunction { get { return o => MasterNamingFunction((TMasterProperty)o); } }
        Func<object, string> IPropertyRunner.SideNamingFunction { get { return o => SideNamingFunction((TSideProperty)o); } }
        public IPropertyRunner Invert()
            => new PropertyRunner<TSide, TMaster, TSideProperty, TMasterProperty>(PropertyName, SideValue, MasterValue, SideNamingFunction, MasterNamingFunction);
    }
}
