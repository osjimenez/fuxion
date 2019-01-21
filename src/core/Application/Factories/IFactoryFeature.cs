using Fuxion.Domain;

namespace Fuxion.Application.Factories
{
	public interface IFactoryFeature<TAggregate> where TAggregate : Aggregate
	{
		void Create(TAggregate aggregate);
	}
}