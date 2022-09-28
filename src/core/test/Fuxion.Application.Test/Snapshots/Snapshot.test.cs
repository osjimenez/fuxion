namespace Fuxion.Application.Test.Snapshots;

using Fuxion.Application.Aggregates;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Reflection;
using Fuxion.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit.Abstractions;

public class SnapshotTest : BaseTest
{
	public SnapshotTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "Snapshot - Serialize")]
	public void Serialize()
	{
		var snapshot = new TestSnapshot();
		snapshot.AggregateId = Guid.NewGuid();
		snapshot.Version = 101;
		snapshot.Id = Guid.NewGuid();
		snapshot.Name = "Test";
		var json = snapshot.ToJson();
		Output.WriteLine(json);
	}
}
[TypeKey(nameof(TestSnapshot))]
public class TestSnapshot : Snapshot<TestAggregate>
{
	[JsonInclude]
	public string? Name { get; internal set; }

	protected internal override void Hydrate(TestAggregate aggregate) => throw new NotImplementedException();
	protected internal override void Load(TestAggregate aggregate) => throw new NotImplementedException();
}
public class TestAggregate : Aggregate { }

