namespace Fuxion.Windows.Data;

using System.Globalization;
using System.Windows;

public class IntegerToVisibilityConverter : GenericConverter<int, Visibility>
{
	public string? VisibleValuesCommaSeparated { get; set; }
	public string? CollapsedValuesCommaSeparated { get; set; }
	public string? HiddenValuesCommaSeparated { get; set; }
	public Visibility NonDeclaredValue { get; set; } = Visibility.Collapsed;
	public override Visibility Convert(int source, CultureInfo culture) => VisibleValuesCommaSeparated?.Split(',').Select(v =>
																		   {
																			   if (int.TryParse(v, out var res))
																				   return res;
																			   return (int?)null;
																		   }).RemoveNulls().Contains(source) ?? false
			? Visibility.Visible
			: CollapsedValuesCommaSeparated?.Split(',').Select(v =>
			{
				if (int.TryParse(v, out var res))
					return res;
				return (int?)null;
			}).RemoveNulls().Contains(source) ?? false
			? Visibility.Collapsed
			: HiddenValuesCommaSeparated?.Split(',').Select(v =>
			{
				if (int.TryParse(v, out var res))
					return res;
				return (int?)null;
			}).RemoveNulls().Contains(source) ?? false
			? Visibility.Hidden
			: NonDeclaredValue;
}