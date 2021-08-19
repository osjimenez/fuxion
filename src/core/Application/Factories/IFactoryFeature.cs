namespace Fuxion.Application.Factories;

using Fuxion.Domain;

public interface IFactoryFeature<TAggregate> where TAggregate : Aggregate
{
	void Create(TAggregate aggregate);
}