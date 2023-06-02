namespace Fuxion.Domain;

// public abstract record Command(Guid Id) { }
public abstract record Command(Guid AggregateId) : IFeaturizable<Command>
{
	IFeatureCollection<Command> IFeaturizable<Command>.Features { get; } = new FeatureCollection<Command>();
}
public static class CommandExtensions
{
	public static IFeaturizable<Command> Features(this Command me) => me.Features<Command>();
}