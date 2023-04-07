using Xunit;

namespace Fuxion.Domain.Test;

public class EventTest
{
	[Fact(DisplayName = "Event - Serialization")]
	public void Serialization()
	{
		var eventJson = """
			{
				"Name: "P1",
				"AggregateId": "99c8a592-b2bd-4845-92dd-d4ba857c13a7"
			}
			""";
		var @event = eventJson.FromJson<MockEvent>();
		Assert.NotNull(@event);
		Assert.NotEqual(Guid.Empty, @event.AggregateId);
	}
}

public record MockEvent : Fuxion.Domain.Event
{
	public MockEvent(Guid AggregateId) : base(AggregateId) { }
	public string? Name { get; set; }
}