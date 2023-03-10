using Castle.Components.DictionaryAdapter;
using Fuxion.Reflection;
using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Domain.Test;

public class FeatureTest : BaseTest<FeatureTest>
{
	public FeatureTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "TryGet")]
	public void TryGet()
	{
		var user = new MockUser(Guid.NewGuid());
		user.Features().Add<MockUserFeature>();
		user.Features().Add<AggregateFeature>();
		user.Features().Add<MockGroupFeature>();
		Assert.True(user.Features().Has<MockUserFeature>());
		Assert.True(user.Attached);
		Assert.True(user.Features().Has<AggregateFeature>());
		user.Features().Remove<AggregateFeature>();
		Assert.False(user.Features().Has<AggregateFeature>());
		user.Features().Remove<MockUserFeature>();
		Assert.False(user.Attached);

		var group = new MockGroup(Guid.NewGuid());
		group.Features().Add<MockGroupFeature>();
		Assert.True(group.Features().Has<MockGroupFeature>());
		Assert.False(user.Features().Has<AggregateFeature>());
		group.Features().Remove<MockGroupFeature>();
	}
}
[TypeKey(nameof(AggregateFeature))]
public class AggregateFeature : IFeature<IAggregate>
{
	
}
public class MockUser : IAggregate, IFeaturizable<MockUser>
{
	public MockUser(Guid id)
	{
		Id = id;
	}
	public bool Attached { get; set; }
	//IFeatureCollection<IAggregate> IFeaturizable<IAggregate>.Features => ((IFeaturizable<MockUser>)this).Features;
	IFeatureCollection<IAggregate> IFeaturizable<IAggregate>.Features => IFeatureCollection<IAggregate>.Create();
	// IFeatureCollection<MockUser> IFeaturizable<MockUser>.Features { get; } = IFeatureCollection<MockUser>.Create();
	IFeatureCollection<MockUser> IFeaturizable<MockUser>.Features => ((IFeaturizable<IAggregate>)this).Features;
	public Guid Id { get; init; }
}
[TypeKey(nameof(MockUserFeature))]
public class MockUserFeature : IFeature<MockUser>
{
	public void OnAttach(MockUser user) => user.Attached = true;
	public void OnDetach(MockUser user) => user.Attached = false;
}


public class MockGroup : IAggregate
{
	public MockGroup(Guid id)
	{
		Id = id;
	}
	IFeatureCollection<IAggregate> IFeaturizable<IAggregate>.Features { get; } = IFeatureCollection<IAggregate>.Create();
	public Guid Id { get; init; }
}
[TypeKey(nameof(MockGroupFeature))]
public class MockGroupFeature : IFeature<IAggregate>
{
	
}