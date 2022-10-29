using System.Diagnostics;

namespace Fuxion.Identity.Test.Mocks;

[DebuggerDisplay("{" + nameof(Name) + "}")]
class StringFunction : IFunction<string>
{
	public StringFunction(string id)
	{
		Id   = id;
		Name = id;
	}
	public IEnumerable<IFunction<string>>        Inclusions { get; } = new List<IFunction<string>>();
	public IEnumerable<IFunction<string>>        Exclusions { get; } = new List<IFunction<string>>();
	public string                                Id         { get; }
	object IFunction.                            Id         => Id;
	public string                                Name       { get; set; }
	IEnumerable<IFunction> IInclusive<IFunction>.Inclusions => Inclusions;
	IEnumerable<IFunction> IExclusive<IFunction>.Exclusions => Inclusions;
}