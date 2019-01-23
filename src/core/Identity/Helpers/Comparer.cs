namespace Fuxion.Identity.Helpers
{
	internal static class Comparer
	{
		internal static bool AreEquals<T>(T o1, T o2)
		{
			if (o1 == null && o2 == null)
				return true;
			if (o1 == null || o2 == null)
				return false;
			if (Equals(o1, o2))
				return true;
			return false;
		}
	}
}
