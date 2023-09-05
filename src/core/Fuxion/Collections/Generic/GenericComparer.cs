namespace Fuxion.Collections.Generic;

public class GenericComparer<T> : Comparer<T>
{
	public GenericComparer(Func<T?, T?, int> comparisonFunction) => this.comparisonFunction = comparisonFunction;
	readonly Func<T?, T?, int> comparisonFunction;
	public override int Compare(T? x, T? y) => comparisonFunction(x, y);
}