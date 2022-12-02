namespace Fuxion.Identity;

public interface IInclusive<T>
{
	IEnumerable<T> Inclusions { get; }
}

public interface IExclusive<T>
{
	IEnumerable<T> Exclusions { get; }
}

public interface IFunction : IInclusive<IFunction>, IExclusive<IFunction>
{
	object Id { get; }
	string Name { get; set; }
}

public interface IFunction<TId> : IFunction, IInclusive<IFunction>, IExclusive<IFunction>
{
	new TId Id { get; }
}