using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Windows.Data
{
	public class EnumTypeToNamedEnumValueListConverter : GenericConverter<object, List<NamedEnumValue>, Type>
	{
		public bool IsAlphabeticallyOrdered { get; set; }
		public override List<NamedEnumValue> Convert(object _, Type enumType, CultureInfo culture)
		{
			var res = Enum.GetValues(enumType).Cast<Enum>().Select(e => new NamedEnumValue(e)).ToList();
			if (IsAlphabeticallyOrdered) res = res.OrderBy(e => e.Name).ToList();
			return res;
		}
	}
}
