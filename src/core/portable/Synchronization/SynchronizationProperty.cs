using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface ISynchronizationProperty
    {
        string PropertyName { get; }
        object MasterValue { get; }
        object SideValue { get; }
    }
    internal class SynchronizationProperty<TMasterProperty, TSideProperty> : ISynchronizationProperty
    {
        public SynchronizationProperty(string propertyName, TMasterProperty masterValue, TSideProperty sideValue)
        {
            PropertyName = propertyName;
            MasterValue = masterValue;
            SideValue = sideValue;
        }
        public string PropertyName { get; set; }
        public TMasterProperty MasterValue { get; }
        object ISynchronizationProperty.MasterValue { get { return MasterValue; } }
        public TSideProperty SideValue { get; }
        object ISynchronizationProperty.SideValue { get { return SideValue; } }
    }
}
