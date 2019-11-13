using Fuxion.Json;
using Fuxion.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Json
{
	public class EventSourcingPodTest : BaseTest
	{
		public EventSourcingPodTest(ITestOutputHelper output) : base(output) { }

		[Fact(DisplayName = "EventSourcingPod - ToJson")]
		public void EventSourcingPod_ToJson()
		{
			var id = Guid.NewGuid();
			var evt = new MockEvent(id, "mockName");
			Assert.Throws<EventFeatureNotFoundException>(() => new EventSourcingPod(evt));
			evt.AddEventSourcingFeature(10);
			var pod = evt.ToEventSourcingPod();
			Assert.True(pod.PayloadHasValue);
			Assert.IsType<MockEvent>(pod.Payload);

			var json = pod.ToJson();
			Output.WriteLine("Serialized json:");
			Output.WriteLine(json);
			Assert.Contains(@"""PayloadKey"": ""MockEvent""", json);
			Assert.Contains(@"""TargetVersion"": 10", json);
			Assert.Contains($@"""AggregateId"": ""{id.ToString()}""", json);
			Assert.Contains(@"""Name"": ""mockName""", json);
		}
		[Fact(DisplayName = "EventSourcingPod - FromJson")]
		public void EventSourcingPod_FromJson()
		{
			var tkd = new TypeKeyDirectory();
			tkd.Register<MockEvent>();

			var id = Guid.Parse("52d307a2-39ba-47e2-b73f-2faf0727d44e");
			var json = @"
			{
				""PayloadKey"": ""MockEvent"",
				""TargetVersion"": 10,
				""Payload"": {
					""AggregateId"": ""52d307a2-39ba-47e2-b73f-2faf0727d44e"",
					""Name"": ""mockName""
				}
			}";

			var pod = json.FromJson<EventSourcingPod>();
			Assert.Equal("MockEvent", pod.PayloadKey);
			Assert.Equal(10, pod.TargetVersion);
			Assert.False(pod.PayloadHasValue);
			Assert.Null(pod.Payload);

			var evt = pod.WithTypeKeyDirectory(tkd);
			Assert.Equal(id, evt?.AggregateId);
			Assert.IsType<MockEvent>(evt);
			var mevt = (MockEvent?)evt;
			Assert.Equal("mockName", mevt?.Name);
			Assert.Equal(10, mevt?.EventSourcing().TargetVersion);
		}
	}
	public abstract class Event
	{
		protected Event(Guid aggregateId) => AggregateId = aggregateId;
		public Guid AggregateId { get; }

		internal List<EventFeature> Features { get; } = new List<EventFeature>();
	}
	public abstract class EventFeature
	{
		public Event? Event { get; internal set; }
	}
	public static class EventFeatureExtensions
	{
		public static void AddFeature<TFeature>(this Event me, Action<TFeature>? initializeAction = null) where TFeature : EventFeature, new()
		{
			var fea = Activator.CreateInstance<TFeature>();
			fea.Event = me;
			initializeAction?.Invoke(fea);
			me.Features.Add(fea);
		}
		public static bool HasFeature<TFeature>(this Event me) where TFeature : EventFeature, new()
			=> me.Features.OfType<TFeature>().Any();
		public static TFeature GetFeature<TFeature>(this Event me) where TFeature : EventFeature, new()
			=> me.Features.OfType<TFeature>().SingleOrDefault();
	}
	public class EventFeatureNotFoundException :FuxionException
	{
		public EventFeatureNotFoundException(string message) : base(message) { }
	}
	[TypeKey("MockEvent")]
	public class MockEvent : Event
	{
		public MockEvent(Guid aggregateId, string name) : base(aggregateId) => Name = name;
		[JsonProperty]
		public string Name { get; private set; }
	}
	public class EventSourcingPod : JsonPod<Event, string>
	{
		[JsonConstructor]
		protected EventSourcingPod() { }
		internal EventSourcingPod(Event @event) : base(@event, @event.GetType().GetTypeKey() ?? @event.GetType().FullName ?? throw new InvalidProgramException($"Event type '{@event.GetType().Name}' hasn't a FullName"))
		{
			if (!@event.HasEventSourcing()) throw new EventFeatureNotFoundException($"'{nameof(EventSourcingPod)}' require '{nameof(EventSourcingEventFeature)}'");
			TargetVersion = @event.EventSourcing().TargetVersion;
		}
		[JsonProperty]
		public int TargetVersion { get; private set; }

		public T AsEvent<T>() where T : Event => base.As<T>().Transform(evt => evt.AddEventSourcingFeature(TargetVersion));
		public Event? AsEvent(Type type) => ((Event?)base.As(type)).Transform(evt => evt?.AddEventSourcingFeature(TargetVersion));
		public Event? WithTypeKeyDirectory(TypeKeyDirectory typeKeyDirectory) => AsEvent(typeKeyDirectory[PayloadKey]);
	}
	public class EventSourcingEventFeature : EventFeature
	{
		public int TargetVersion { get; internal set; }
	}
	public static class EventSourcingEventFeatureExtensions
	{
		public static bool HasEventSourcing(this Event me)
			=> me.HasFeature<EventSourcingEventFeature>();
		public static EventSourcingEventFeature EventSourcing(this Event me)
			=> me.GetFeature<EventSourcingEventFeature>();
		public static void AddEventSourcingFeature(this Event me, int targetVersion)
			=> me.AddFeature<EventSourcingEventFeature>(esef => esef.TargetVersion = targetVersion);
		public static EventSourcingPod ToEventSourcingPod(this Event me) => new EventSourcingPod(me);
	}
}
