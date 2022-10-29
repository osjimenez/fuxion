using Fuxion.Identity.Helpers;

namespace Fuxion.Identity;

class FunctionEqualityComparer : IEqualityComparer<IFunction>
{
	public bool Equals(IFunction? x, IFunction? y) => AreEquals(x, y);
	public int GetHashCode(IFunction obj)
	{
		if (obj == null) return 0;
		return obj.Id.GetHashCode();
	}
	static bool AreEquals(object? obj1, object? obj2)
	{
		// If both are NULL, return TRUE
		if (Equals(obj1, null) && Equals(obj2, null)) return true;
		// If some of them is null, return FALSE
		if (Equals(obj1, null) || Equals(obj2, null)) return false;
		// If any of them are of other type, return FALSE
		if (!(obj1 is IFunction) || !(obj2 is IFunction)) return false;
		var fun1 = (IFunction)obj1;
		var fun2 = (IFunction)obj2;
		// Compare the ids
		return Comparer.AreEquals(fun1.Id, fun2.Id);
	}
}