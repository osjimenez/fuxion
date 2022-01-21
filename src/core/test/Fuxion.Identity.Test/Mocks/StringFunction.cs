namespace Fuxion.Identity.Test.Mocks;

using System.Diagnostics;

[DebuggerDisplay("{" + nameof(Name) + "}")]
internal class StringFunction : IFunction<string>
{
	public StringFunction(string id) { Id = id; Name = id; }

	public string Id { get; private set; }
	object IFunction.Id => Id;

	public string Name { get; set; }

	public IEnumerable<IFunction<string>> Inclusions { get; private set; } = new List<IFunction<string>>();
	IEnumerable<IFunction> IInclusive<IFunction>.Inclusions => Inclusions;

	public IEnumerable<IFunction<string>> Exclusions { get; private set; } = new List<IFunction<string>>();
	IEnumerable<IFunction> IExclusive<IFunction>.Exclusions => Inclusions;
}