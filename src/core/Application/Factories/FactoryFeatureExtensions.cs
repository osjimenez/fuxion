using Fuxion.Domain;
using System.Linq;

namespace Fuxion.Application.Factories
{
	public static class FactoryFeatureExtensions
	{
		public static bool HasFeature<TAggregate, TFactoryFeature>(this Factory<TAggregate> me)
			where TAggregate : Aggregate, new()
			where TFactoryFeature : IFactoryFeature<TAggregate> => me.Features.OfType<TFactoryFeature>().Any();
		public static TFactoryFeature GetFeature<TAggregate, TFactoryFeature>(this Factory<TAggregate> me)
			where TAggregate : Aggregate, new()
			where TFactoryFeature : IFactoryFeature<TAggregate> => me.Features.OfType<TFactoryFeature>().Single();
	}
}