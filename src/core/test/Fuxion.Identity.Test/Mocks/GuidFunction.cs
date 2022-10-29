using System.Diagnostics;

namespace Fuxion.Identity.Test.Mocks;

[DebuggerDisplay("{" + nameof(Name) + "}")]
class GuidFunction : IFunction<Guid>
{
	public GuidFunction(Guid id, string name)
	{
		Id   = id;
		Name = name;
	}
	public IEnumerable<IFunction<Guid>>          Inclusions { get; set; } = new List<IFunction<Guid>>();
	public IEnumerable<IFunction<Guid>>          Exclusions { get; set; } = new List<IFunction<Guid>>();
	public Guid                                  Id         { get; }
	object IFunction.                            Id         => Id;
	public string                                Name       { get; set; }
	IEnumerable<IFunction> IInclusive<IFunction>.Inclusions => Inclusions;
	IEnumerable<IFunction> IExclusive<IFunction>.Exclusions => Exclusions;
}