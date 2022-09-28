﻿namespace Fuxion.Application.Test.Events;

using Fuxion.Application.Events;
using Fuxion.Domain.Events;
using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Testing;
using System.Net.Mime;
using Xunit.Abstractions;

public class EventSourcingPodTest : BaseTest
{
	public EventSourcingPodTest(ITestOutputHelper output) : base(output) { }

	[Fact(DisplayName = "EventSourcingPod - ToJson")]
	public void ToJson()
	{
		var id = Guid.NewGuid();
		var evt = new EventBase(id)
		{
			Name = "mockName"
		};
		Assert.Throws<EventFeatureNotFoundException>(() => new EventSourcingPod(evt));
		//evt.AddEventSourcingFeature(10);
		evt.AddEventSourcing(10, Guid.NewGuid(), DateTime.UtcNow, 11);
		var pod = evt.ToEventSourcingPod();
		Assert.True(pod.PayloadHasValue);
		Assert.IsType<EventBase>(pod.Payload);

		var json = pod.ToJson();
		Output.WriteLine("Serialized json:");
		Output.WriteLine(json);
		Assert.Contains(@"""PayloadKey"": ""EventBase""", json);
		Assert.Contains(@"""TargetVersion"": 10", json);
		Assert.Contains($@"""AggregateId"": ""{id}""", json);
		Assert.Contains(@"""Name"": ""mockName""", json);
	}
	[Fact(DisplayName = "EventSourcingPod - FromJson")]
	public void FromJson()
	{
		var tkd = new TypeKeyDirectory();
		tkd.Register<EventBase>();

		var id = Guid.Parse("52d307a2-39ba-47e2-b73f-2faf0727d44e");
		var json = """
		{"TargetVersion": 10,
			"CorrelationId": "68d9e3a4-558d-47c7-a058-38c599a6b116",
			"EventCommittedTimestamp": "2022-09-28T14:12:12.8459732Z",
			"ClassVersion": 11,
			"PayloadKey": "EventBase",
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
		Output.WriteLine(pod.PayloadKey);
		Assert.Equal("EventBase", pod.PayloadKey);
		Assert.Equal(10, pod.TargetVersion);
		Assert.False(pod.PayloadHasValue);
		Assert.Null(pod.Payload);

		var evt = pod.WithTypeKeyDirectory(tkd);
		Assert.Equal(id, evt?.AggregateId);
		Assert.IsType<EventBase>(evt);
		var mevt = (EventBase?)evt;
		Assert.Equal("mockName", mevt?.Name);
		Assert.Equal(10, mevt?.EventSourcing().TargetVersion);
	}
}
