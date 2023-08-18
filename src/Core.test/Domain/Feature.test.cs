using Fuxion.Reflection;
using Fuxion.Xunit;
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

[UriKey(UriKey.FuxionBaseUri + $"test/{nameof(AggregateFeature)}/1.0.0")]
public class AggregateFeature : IFeature<IAggregate>
{
	public void OnAttach(IAggregate featurizable) { }
	public void OnDetach(IAggregate featurizable) { }
}

public class MockUser : 
#if NET462
	Featurizable<MockAggregate>,
#endif
	IAggregate, IFeaturizable<MockUser>
{
	public MockUser(Guid id)
	{
		Id = id;
	}
	public bool Attached { get; set; }
	//IFeatureCollection<IAggregate> IFeaturizable<IAggregate>.Features => ((IFeaturizable<MockUser>)this).Features;
	IFeatureCollection<IAggregate> IFeaturizable<IAggregate>.Features { get; } = 
#if NET462
		new FeatureCollection<IAggregate>();
#else
		IFeatureCollection<IAggregate>.Create();
#endif
	// IFeatureCollection<MockUser> IFeaturizable<MockUser>.Features { get; } = IFeatureCollection<MockUser>.Create();
	IFeatureCollection<MockUser> IFeaturizable<MockUser>.Features => ((IFeaturizable<IAggregate>)this).Features;
	public Guid Id
	{
		get;
#if !NET462
		init;
#endif
	}
#if NET462
		= Guid.NewGuid();
#endif
}
[UriKey(UriKey.FuxionBaseUri + $"test/{nameof(MockUserFeature)}/1.0.0")]
public class MockUserFeature : IFeature<MockUser>
{
	public void OnAttach(MockUser user) => user.Attached = true;
	public void OnDetach(MockUser user) => user.Attached = false;
}


public class MockGroup : 
#if NET462
	Featurizable<MockAggregate>,
#endif
	IAggregate
{
	public MockGroup(Guid id)
	{
		Id = id;
	}
	IFeatureCollection<IAggregate> IFeaturizable<IAggregate>.Features { get; } = 
#if NET462
		new FeatureCollection<IAggregate>();
#else
		IFeatureCollection<IAggregate>.Create();
#endif
	public Guid Id { get; 
#if !NET462
		init;
#endif
	}
#if NET462
		= Guid.NewGuid();
#endif
}
[UriKey(UriKey.FuxionBaseUri + $"test/{nameof(MockGroupFeature)}/1.0.0")]
public class MockGroupFeature : IFeature<IAggregate>
{
	public void OnAttach(IAggregate featurizable) { }
	public void OnDetach(IAggregate featurizable) { }
}