using System.Text.Json;
using Fuxion.Application.Events;
using Fuxion.Domain;
using Fuxion.Json;
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
		var tkr = new TypeKeyDirectory();
		tkr.Register<BaseEvent>();
		tkr.Register<EventSourcingEventFeature>();
		var id = Guid.Parse("197c7b29-ee75-424f-ba9c-bfdcc2d150ea");
		var json = $$"""
			{
				"Discriminator": "Fuxion/Application/Test/Events/BaseEvent",
				"Headers": [
					{
						"Discriminator": "Fuxion/Application/Events/EventSourcingEventFeature",
						"Payload": {
							"TargetVersion": 10,
							"CorrelationId": "f749c151-e079-4a12-a64e-0e41229b6b15",
							"EventCommittedTimestamp": "2023-03-07T14:19:26.5402172Z",
							"ClassVersion": 10,
							"IsReplay": false
						}
					}
				],
				"Payload": {
					"Name": "mockName",
					"Age": 10,
					"AggregateId": "197c7b29-ee75-424f-ba9c-bfdcc2d150ea"
				}
			}
		""";
		JsonSerializerOptions options = new();
		options.Converters.Add(new FeaturizablePodConverterFactory(tkr));
		var pod = json.FromJson<FeaturizablePod<BaseEvent>>(options:options);
		// var pod = json.FromJson<EventSourcingPod>();
		Output.WriteLine(json);
		Assert.NotNull(pod);
		Output.WriteLine(pod.Discriminator.ToString());
		Assert.Equal(new[] { nameof(Fuxion), nameof(Application), nameof(Test), nameof(Events), nameof(BaseEvent) }, pod.Discriminator);
		var esHeader = pod.Headers[typeof(EventSourcingEventFeature).GetTypeKey()].CastWithPayload<EventSourcingEventFeature>();
		// var esHeader = pod.Headers.GetPod<EventSourcingEventFeature>();
		Assert.NotNull(esHeader?.Payload);
		Assert.Equal(10, esHeader.Payload.TargetVersion);
		// Assert.Equal(10, pod.TargetVersion);
		//Assert.False(pod.PayloadHasValue);
		//Assert.Null(pod.Payload);
		
		// var evt = (BaseEvent?)pod.WithTypeKeyResolver(tkr, options);
		var evt = pod.Payload;
		
		Assert.NotNull(evt);
		Assert.Equal(id, evt.AggregateId);
		Assert.IsType<BaseEvent>(evt);
		var esFeature = evt.Features().Get<EventSourcingEventFeature>();
		Assert.Equal("mockName", evt.Name);
		Assert.Equal(10, esFeature.TargetVersion);
		// Assert.Equal(10, mevt?.EventSourcing().TargetVersion);
	}
	[Fact(DisplayName = "EventSourcingPod - ToJson")]
	public void ToJson()
	{
		var tkr = new TypeKeyDirectory();
		tkr.Register<BaseEvent>();
		tkr.Register<EventSourcingEventFeature>();
		var id = Guid.NewGuid();
		var evt = new BaseEvent(id, "mockName", 10);
		// Assert.Throws<FeatureNotFoundException>(() => new EventSourcingPod(evt));
		evt.Features().Add<EventSourcingEventFeature>(e =>
		{
			e.TargetVersion = 10;
			e.CorrelationId = Guid.NewGuid();
			e.EventCommittedTimestamp = DateTime.UtcNow;
			e.ClassVersion = 10;
		});
		// evt.AddEventSourcing(10, Guid.NewGuid(), DateTime.UtcNow, 11);
		var pod = new FeaturizablePod<BaseEvent>(evt);
		// var pod = evt.ToEventSourcingPod();
		Assert.True(pod.PayloadHasValue);
		Assert.IsType<BaseEvent>(pod.Payload);
		JsonSerializerOptions options = new();
		options.WriteIndented = true;
		options.Converters.Add(new FeaturizablePodConverterFactory(tkr));
		var json = pod.ToJson(options);
		Output.WriteLine("Serialized json:");
		Output.WriteLine(json);
		Assert.Contains($@"""Discriminator"": ""Fuxion/Application/Test/Events/BaseEvent""", json);
		Assert.Contains(@"""TargetVersion"": 10", json);
		Assert.Contains($@"""AggregateId"": ""{id}""", json);
		Assert.Contains(@"""Name"": ""mockName""", json);
	}
}