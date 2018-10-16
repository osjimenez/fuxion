using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fuxion.Windows.Data
{
	public class IntegerToVisibilityConverter : GenericConverter<int, Visibility>
	{
		public string VisibleValuesCommaSeparated { get; set; }
		public string CollapsedValuesCommaSeparated { get; set; }
		public string HiddenValuesCommaSeparated { get; set; }
		public Visibility NonDeclaredValue { get; set; } = Visibility.Collapsed;
		public override Visibility Convert(int source, CultureInfo culture)
		{
			return 
				VisibleValuesCommaSeparated.Split(',').Select(v =>
				{
					if (int.TryParse(v, out int res))
						return res;
					return (int?)null;
				}).RemoveNulls().Contains(source) 
				? Visibility.Visible
				: CollapsedValuesCommaSeparated.Split(',').Select(v =>
				{
					if (int.TryParse(v, out int res))
						return res;
					return (int?)null;
				}).RemoveNulls().Contains(source) 
				? Visibility.Collapsed
				: HiddenValuesCommaSeparated.Split(',').Select(v =>
				{
					if (int.TryParse(v, out int res))
						return res;
					return (int?)null;
				}).RemoveNulls().Contains(source) 
				? Visibility.Hidden
				: NonDeclaredValue;
		}
	}
}
