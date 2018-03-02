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
					return culture.TextInfo.ToUpper(source);
				case StringCapitalization.ToLower:
					return culture.TextInfo.ToLower(source);
				case StringCapitalization.ToTitleCase:
					return culture.TextInfo.ToTitleCase(source);
				case StringCapitalization.ToCamelCase:
					return culture.TextInfo.ToTitleCase(source).Replace(" ", "").Transform(s => s.Substring(0, 1).ToLower() + s.Substring(1, s.Length - 1));
				case StringCapitalization.ToPascalCase:
					return culture.TextInfo.ToTitleCase(source).Replace(" ","");
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
