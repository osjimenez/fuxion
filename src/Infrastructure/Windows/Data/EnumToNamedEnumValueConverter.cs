using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Fuxion.Windows.Data
{
	public class EnumToNamedEnumValueConverter : GenericConverter<Enum, NamedEnumValue>
	{
		public override NamedEnumValue Convert(Enum source, CultureInfo culture) => new NamedEnumValue(source);
		public override Enum ConvertBack(NamedEnumValue result, CultureInfo culture) => result.Value;
	}
}
