namespace Fuxion.Domain.Test;

using Xunit;

public class EventTest
{
	[Fact(DisplayName = "Event - Serialization")]
	public void Serialization()
	{
		var eventJson = @"
			{
				""Name"": ""P1"",
				""AggregateId"": ""99c8a592-b2bd-4845-92dd-d4ba857c13a7""
			}";
		var @event = eventJson.FromJson<MockEvent>();
		// NULLABLE - Prefer Assert.NotNull() but the nullable constraint attribute is missing
		if (@event == null) throw new NullReferenceException($"'@event' variable is null");
		Assert.NotEqual(Guid.Empty, @event.AggregateId);
	}
}
public record MockEvent : Event
{
	public MockEvent(Guid id) : base(id) { }
	public string? Name { get; set; }
}