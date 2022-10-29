namespace Fuxion.Collections.Generic;

public class GenericEqualityComparer<T> : EqualityComparer<T> {
	public GenericEqualityComparer(Func<T?, T?, bool> equalsFunction, Func<T, int> getHashCodeFunction)
	{
		this.equalsFunction      = equalsFunction;
		this.getHashCodeFunction = getHashCodeFunction;
	}
	readonly        Func<T?, T?, bool> equalsFunction;
	readonly        Func<T, int>       getHashCodeFunction;
	public override bool               Equals(T?     x, T? y) => equalsFunction.Invoke(x, y);
	public override int                GetHashCode(T obj) => getHashCodeFunction.Invoke(obj);
}