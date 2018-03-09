using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
	[ValueConversion(typeof(string), typeof(string))]
	public class StringToCapitalizedStringConverter : GenericConverter<string, string>
	{
		public StringCapitalization Capitalization { get; set; }
		public override string Convert(string source, CultureInfo culture)
		{
			switch (Capitalization)
			{
				case StringCapitalization.ToUpper:
					return source.ToUpper(culture);
				case StringCapitalization.ToLower:
					return source.ToLower(culture);
				case StringCapitalization.ToTitleCase:
					return source.ToTitleCase(culture);
				case StringCapitalization.ToCamelCase:
					return source.ToCamelCase(culture);
				case StringCapitalization.ToPascalCase:
					return source.ToPascalCase(culture);
				default:
					return source;
			}
		}
	}
	public enum StringCapitalization
	{
		ToUpper,
		ToLower,
		ToTitleCase,
		ToCamelCase,
		ToPascalCase
	}
}
