namespace Fuxion.Domain.Aggregates;

public static class AggregateFeatureExtensions
{
	public static bool HasFeature<TFeature>(this Aggregate aggregate) where TFeature : IAggregateFeature
		=> aggregate.Features.OfType<TFeature>().Any();
	public static TFeature GetFeature<TFeature>(this Aggregate aggregate) where TFeature : IAggregateFeature
		=> aggregate.Features.OfType<TFeature>().SingleOrDefault() ?? throw new AggregateFeatureNotFoundException($"'{typeof(TFeature).Name}' must be present in the aggregate");
	public static void AttachFeature<TFeature>(this Aggregate aggregate, TFeature feature) where TFeature : IAggregateFeature
	{
		if (HasFeature<TFeature>(aggregate))
			throw new AggregateFeatureAlreadyExistException($"Feature cannot be added to aggregate '{aggregate.GetType().Name}:{aggregate.Id}' because already has the feature '{typeof(TFeature).Name}'");
		aggregate.Features.Add(feature);
		feature.OnAttach(aggregate);
	}
}