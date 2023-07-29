#if false
using System.Text.Json.Serialization;
using Fuxion.Domain;
using Fuxion.Reflection;
using Fuxion.Testing;
using Xunit.Abstractions;

namespace Fuxion.Application.Test.Snapshots;

public class SnapshotTest : BaseTest<SnapshotTest>
{
	public SnapshotTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "Snapshot - Serialize")]
	public void Serialize()
	{
		var snapshot = new TestSnapshot
		{
			AggregateId = Guid.NewGuid(),
			Version = 101,
			Id = Guid.NewGuid(),
			Name = "Test"
		};
		var json = snapshot.SerializeToJson();
		Output.WriteLine(json);
	}
}
[UriKey(UriKey.FuxionBaseUri + $"test/{nameof(TestSnapshot)}/1.0.0")]
public class TestSnapshot : Snapshot<TestAggregate>
{
	[JsonInclude]
	public string? Name { get; internal set; }
	protected internal override void Hydrate(TestAggregate aggregate) => throw new NotImplementedException();
	protected internal override void Load(TestAggregate aggregate) => throw new NotImplementedException();
}

public class TestAggregate : IAggregate
{
	public Guid Id { get; init; }
	IFeatureCollection<IAggregate> IFeaturizable<IAggregate>.Features { get; } = IFeatureCollection<IAggregate>.Create();
}
#endif