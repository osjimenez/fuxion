namespace Fuxion.Domain;

// public abstract record Command(Guid Id) { }
public abstract 
#if NETSTANDARD2_0 || NET462
	class
#else
	record
#endif
	Command(Guid aggregateId) : 
#if NETSTANDARD2_0 || NET462
	Featurizable<Command>,
#endif
	IFeaturizable<Command>
{
	public Guid AggregateId { get; set; } = aggregateId;
	IFeatureCollection<Command> IFeaturizable<Command>.Features { get; } = new FeatureCollection<Command>();
}
public static class CommandExtensions
{
	public static IFeaturizable<Command> Features(this Command me) => me.Features<Command>();
}