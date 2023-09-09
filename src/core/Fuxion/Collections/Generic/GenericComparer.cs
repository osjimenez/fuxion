namespace Fuxion.Collections.Generic;

public class GenericComparer<T>(Func<T?, T?, int> comparisonFunction) : Comparer<T>
{
	public override int Compare(T? x, T? y) => comparisonFunction(x, y);
}