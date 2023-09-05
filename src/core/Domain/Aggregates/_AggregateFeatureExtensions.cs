// namespace Fuxion.Domain.Aggregates;
//
// public static class AggregateFeatureExtensions
// {
// 	public static bool HasFeature<TFeature>(this IAggregate aggregate) where TFeature : IFeature<IAggregate> => aggregate.Features.Has<TFeature>();
// 	public static TFeature GetFeature<TFeature>(this IAggregate aggregate) where TFeature : IFeature<IAggregate> =>
// 		aggregate.Features.Get<TFeature>();// ?? throw new AggregateFeatureNotFoundException($"'{typeof(TFeature).Name}' must be present in the aggregate");
// 	public static void AttachFeature<TFeature>(this IAggregate aggregate, TFeature feature) where TFeature : IFeature<IAggregate>
// 	{
// 		if (HasFeature<TFeature>(aggregate))
// 			throw new AggregateFeatureAlreadyExistException($"Feature cannot be added to aggregate '{aggregate.GetType().Name}:{aggregate.Id}' because already has the feature '{typeof(TFeature).Name}'");
// 		aggregate.Features.Add(feature);
// 		feature.OnAttach(aggregate);
// 	}
// }