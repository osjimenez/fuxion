namespace Fuxion.Identity;

[Flags]
public enum ScopePropagation
{
	ToExclusions = 1,
	ToMe         = 2,
	ToInclusions = 4
}