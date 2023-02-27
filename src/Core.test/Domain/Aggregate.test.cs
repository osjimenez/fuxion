using Fuxion.Domain.Aggregates;
using Fuxion.Domain.Events;
using Moq;
using Xunit;

namespace Fuxion.Domain.Test;

public class AggregateTest
{
	[Fact(DisplayName = "Aggregate - ApplyEvent")]
	public void ApplyEvent()
	{
		var agg = new Mock<MockAggregate>();
		var evt = new TestedEvent(agg.Object.Id);
		Assert.Throws<AggregateFeatureNotFoundException>(() => agg.Object.ApplyEvent(evt));
		agg.Object.AttachEvents();
		agg.Object.ApplyEvent(evt);
		Assert.Throws<AggregateStateMismatchException>(() => agg.Object.ApplyEvent(new TestedEvent(Guid.NewGuid())));
		Assert.Throws<AggregateApplyEventMethodMissingException>(() => agg.Object.ApplyEvent(new Mock<Event>(agg.Object.Id).Object));
		agg.Verify(a => a.WhenTested(It.IsAny<TestedEvent>()), Times.Once);
		Assert.Single(agg.Object.GetPendingEvents());
	}
}

public record TestedEvent : Event
{
	public TestedEvent(Guid aggregateId) : base(aggregateId) { }
}

public class MockAggregate : Aggregate
{
	[AggregateEventHandler]
	public virtual void WhenTested(TestedEvent @event) { }
}