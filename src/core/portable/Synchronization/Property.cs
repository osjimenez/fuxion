using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class Property<TMasterProperty, TSideProperty> : IProperty
    {
        public Property(string propertyName, TMasterProperty masterValue, TSideProperty sideValue)
        {
            PropertyName = propertyName;
            MasterValue = masterValue;
            SideValue = sideValue;
        }
        public string PropertyName { get; set; }
        public TMasterProperty MasterValue { get; }
        object IProperty.MasterValue { get { return MasterValue; } }
        public TSideProperty SideValue { get; }
        object IProperty.SideValue { get { return SideValue; } }
    }
}
