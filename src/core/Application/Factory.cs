using System.Dynamic;
using Fuxion.Application.Factories;
using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fuxion.Application;

public abstract class Factory<TAggregate> : IFeaturizable<Factory<TAggregate>> where TAggregate : IAggregate, new()
{
	// public Factory(IServiceProvider serviceProvider) => Features = serviceProvider.GetServices<IFactoryFeature<TAggregate>>();
	// internal IEnumerable<IFactoryFeature<TAggregate>> Features { get; }
	public TAggregate Create(Guid id)
	{
		var agg = new TAggregate {
			Id = id
		};
		foreach (var feature in this.Features().All<IFactoryFeature<TAggregate>>()) feature.Initialize(agg);
		return agg;
	}
	IFeatureCollection<Factory<TAggregate>> IFeaturizable<Factory<TAggregate>>.Features { get; } = IFeatureCollection<Factory<TAggregate>>.Create();
}
public static class FactoryExtensions
{
	public static IFeaturizable<Factory<TAggregate>> Features<TAggregate>(this Factory<TAggregate> me) where TAggregate : IAggregate, new() => me.Features<Factory<TAggregate>>();
}
public interface IFactoryFeature<TAggregate> : IFeature<Factory<TAggregate>> where TAggregate : IAggregate, new()
{
	protected Factory<TAggregate>? Factory { get; set; }
	TAggregate Create(Guid id)
	{
		if(Factory is null)
			throw new InvalidProgramException($"Factory is null in '{typeof(IFactoryFeature<TAggregate>).GetSignature()}'");
		return Factory.Create(id);
	}
	void Initialize(TAggregate aggregate);
	void IFeature<Factory<TAggregate>>.OnAttach(Factory<TAggregate> factory) => Factory = factory;
}