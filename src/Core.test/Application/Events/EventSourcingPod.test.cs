using Fuxion.Application.Events;
using Fuxion.Domain.Events;
using Fuxion.Reflection;
using Fuxion.Testing;
using Xunit.Abstractions;

namespace Fuxion.Application.Test.Events;

public class EventSourcingPodTest : BaseTest<EventSourcingPodTest>
{
	public EventSourcingPodTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "EventSourcingPod - FromJson")]
	public void FromJson()
	{
		var tkd = new TypeKeyDirectory();
		tkd.Register<BaseEvent>();
		var id = Guid.Parse("52d307a2-39ba-47e2-b73f-2faf0727d44e");
		var json = $$"""
		{
			"TargetVersion": 10,
			"CorrelationId": "68d9e3a4-558d-47c7-a058-38c599a6b116",
			"EventCommittedTimestamp": "2022-09-28T14:12:12.8459732Z",
			"ClassVersion": 11,
			"Discriminator": "Fuxion/Application/Test/Events/BaseEvent",
			"Payload": {
				"Name": "mockName",
				"Age": 0,
				"AggregateId": "52d307a2-39ba-47e2-b73f-2faf0727d44e"
			}
		}
		""";
		var pod = json.FromJson<EventSourcingPod>();
		Output.WriteLine(json);
		Assert.NotNull(pod);
		Output.WriteLine(pod.Discriminator.ToString());
		Assert.Equal(new[] { nameof(Fuxion), nameof(Application), nameof(Test), nameof(Events), nameof(BaseEvent) }, pod.Discriminator);
		Assert.Equal(10, pod.TargetVersion);
		Assert.False(pod.PayloadHasValue);
		Assert.Null(pod.Payload);
		var evt = pod.WithTypeKeyDirectory(tkd);
		Assert.Equal(id, evt?.AggregateId);
		Assert.IsType<BaseEvent>(evt);
		var mevt = (BaseEvent?)evt;
		Assert.Equal("mockName", mevt?.Name);
		Assert.Equal(10, mevt?.EventSourcing().TargetVersion);
	}
	[Fact(DisplayName = "EventSourcingPod - ToJson")]
	public void ToJson()
	{
		var id = Guid.NewGuid();
		var evt = new BaseEvent(id, "mockName", 10);
		Assert.Throws<EventFeatureNotFoundException>(() => new EventSourcingPod(evt));
		evt.AddEventSourcing(10, Guid.NewGuid(), DateTime.UtcNow, 11);
		var pod = evt.ToEventSourcingPod();
		Assert.True(pod.PayloadHasValue);
		Assert.IsType<BaseEvent>(pod.Payload);
		var json = pod.ToJson();
		Output.WriteLine("Serialized json:");
		Output.WriteLine(json);
		Assert.Contains($@"""Discriminator"": ""Fuxion/Application/Test/Events/BaseEvent""", json);
		Assert.Contains(@"""TargetVersion"": 10", json);
		Assert.Contains($@"""AggregateId"": ""{id}""", json);
		Assert.Contains(@"""Name"": ""mockName""", json);
	}
}