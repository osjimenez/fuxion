using Fuxion.Application.Aggregates;
using Fuxion.Domain;

namespace Fuxion.Application.Factories;

public class SnapshotFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : Aggregate
{
	public SnapshotFactoryFeature(Type type, int frecuency)
	{
		Type = type;
		Frecuency = frecuency;
	}
	internal int Frecuency { get; set; }
	internal Type Type { get; set; }
	public void Create(TAggregate aggregate)
	{
		aggregate.EventSourcing().SnapshotType = Type;
		aggregate.EventSourcing().SnapshotFrequency = Frecuency;
	}
}