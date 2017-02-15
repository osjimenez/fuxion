using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class PropertyRunner<TMasterProperty, TSideProperty> : IPropertyRunner
    {
        public PropertyRunner(string propertyName, TMasterProperty masterValue, TSideProperty sideValue)
        {
            PropertyName = propertyName;
            MasterValue = masterValue;
            SideValue = sideValue;
        }
        public string PropertyName { get; set; }
        public TMasterProperty MasterValue { get; }
        object IPropertyRunner.MasterValue { get { return MasterValue; } }
        public TSideProperty SideValue { get; }
        object IPropertyRunner.SideValue { get { return SideValue; } }
    }
}
