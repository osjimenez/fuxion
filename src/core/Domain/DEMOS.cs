#if !NETSTANDARD2_0 && !NET462
using System.Dynamic;
using Microsoft.Extensions.DependencyInjection;

namespace Fuxion.Domain.Demos;

public class a
{
	public a()
	{
		EventFactory<MyEvent> fac = new();
		var evt = fac.Create();

		IServiceProvider sp = default!;
		AggregateFactory<MyAggregate> fac2 = new(sp);
		var agg = fac2.Create();
	}
}

public abstract class Factory<TFeaturizable> : IFeaturizable<Factory<TFeaturizable>> where TFeaturizable : IFeaturizable<TFeaturizable>
{
	public abstract TFeaturizable Create();
	IFeatureCollection<Factory<TFeaturizable>> IFeaturizable<Factory<TFeaturizable>>.Features { get; } = IFeatureCollection<Factory<TFeaturizable>>.Create();
}

public abstract class ServiceProviderFactory<TFeaturizable> : Factory<TFeaturizable> where TFeaturizable : IFeaturizable<TFeaturizable>
{
	readonly IServiceProvider _serviceProvider;
	public ServiceProviderFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}
	public override TFeaturizable Create() => _serviceProvider.GetRequiredService<TFeaturizable>();
}
// EVENTS
public interface IEvent<in TEvent> : IFeaturizable<TEvent> where TEvent : IFeaturizable<TEvent> { }
public class EventFactory<TEvent> : Factory<TEvent> where TEvent : IEvent<TEvent>
{
	public override TEvent Create() => throw new NotImplementedException();
}
public abstract record EventBase(Guid AggregateId) : IEvent<EventBase>
{
	IFeatureCollection<EventBase> IFeaturizable<EventBase>.Features { get; } = new FeatureCollection<EventBase>();
}

public record MyEvent(Guid AggregateId) : EventBase(AggregateId) { }


// AGGREGATES
public interface IAggregate<in TAggregate> : IFeaturizable<TAggregate> where TAggregate : IFeaturizable<TAggregate>
{
	Guid Id { get; init; }
}
public class AggregateFactory<TAggregate> : ServiceProviderFactory<TAggregate> where TAggregate : IAggregate<TAggregate>
{
	public AggregateFactory(IServiceProvider serviceProvider):base(serviceProvider){}
}
public class MyAggregate : IAggregate<MyAggregate>
{
	IFeatureCollection<MyAggregate> IFeaturizable<MyAggregate>.Features { get; } = IFeatureCollection<MyAggregate>.Create();
	public Guid Id { get; init; }
}
#endif