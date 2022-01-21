namespace Fuxion.Windows.Data;

using System.Collections;
using System.Globalization;

public sealed class ICollectionCountToBooleanConverter : GenericConverter<ICollection, bool>
{
	public bool ZeroValue { get; set; } = false;
	public bool NotZeroValue { get; set; } = true;
	public bool NullValue { get; set; } = false;
	public override bool Convert(ICollection source, CultureInfo culture)
	{
		if (source == null) return NullValue;
		if (source.Count == 0) return ZeroValue;
		return NotZeroValue;
	}
}