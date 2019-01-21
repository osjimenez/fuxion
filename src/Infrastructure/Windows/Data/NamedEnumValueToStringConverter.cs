using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Windows.Data
{
	public class NamedEnumValueToStringConverter : GenericConverter<NamedEnumValue, string>
	{
		public override string Convert(NamedEnumValue source, CultureInfo culture) => source.ToString();
	}
}
