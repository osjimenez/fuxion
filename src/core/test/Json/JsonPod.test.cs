using Fuxion.Json;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Json
{
	public class JsonPodTest : BaseTest
	{
		public JsonPodTest(ITestOutputHelper output) : base(output) { }

		[Fact(DisplayName = "JsonPod - ToJson")]
		public void ToJson()
		{
			var payload = new PayloadDerived
			{
				Name = "payloadName",
				Age = 23,
				Nick = "payloadNick"
			};
			var pod = payload.ToJsonPod("podKey", "podName");
			string json = pod.ToJson();

			Output.WriteLine("Serialized json:");
			Output.WriteLine(json);

			Assert.Contains(@"""Key"": ""podKey""", json);
			Assert.Contains(@"""Name"": ""podName""", json);
			Assert.Contains(@"""Name"": ""payloadName""", json);
			Assert.Contains(@"""Age"": 23", json);
			Assert.Contains(@"""Nick"": ""payloadNick""", json);
		}
		[Fact(DisplayName = "JsonPod - FromJson")]
		public void FromJson()
		{
			var json = @"
			{
				""Name"": ""podName"",
				""Key"": ""podKey"",
				""Payload"": {
					""Name"": ""payloadName"",
					""Age"": 23,
					""Nick"": ""payloadNick""
				}
			}";

			Output.WriteLine("json to deserialize:");
			Output.WriteLine(json);

			var pod = json.FromJsonPod<PayloadBase, string>();

			void AssertBase(PayloadBase payload)
			{
				Assert.Equal("payloadName", payload.Name);
				Assert.Equal(23, payload.Age);
			}
			void AssertDerived(PayloadDerived payload)
			{
				AssertBase(payload);
				Assert.Equal("payloadNick", payload.Nick);
			}

			Assert.Equal("podKey", pod.Key);
			Assert.Equal("podName", pod.Name);
			AssertBase(pod);
			Assert.Throws<InvalidCastException>(() => AssertDerived((PayloadDerived)pod));
			AssertDerived(pod.CastToPayload<PayloadDerived>());
		}
		[Fact(DisplayName = "JsonPod - Event")]
		public void Event()
		{
			var evt = new MockEvent { Name = "MockEvent" };
			var pod = evt.ToJsonPod("MockEvent");
			var eventPod = evt.ToJsonPod<IEvent, string>("MockEvent");

			void ProcessMockEvent(MockEvent e) { }
			void ProcessEvent(IEvent e) { }
			ProcessMockEvent(pod);
			var ee = (IEvent)eventPod;

			//ProcessEvent(eventPod);

			Assert.True(true);
		}
	}
	public class PayloadBase
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}
	public class PayloadDerived : PayloadBase
	{
		public string Nick { get; set; }
	}
	public interface IEvent
	{
		Guid AggId { get; }
	}
	public class MockEvent : IEvent
	{
		public Guid AggId { get; set; } = Guid.NewGuid();
		public string Name { get; set; }
	}
	//public class EventDecorator : JsonPod<IEvent, string>, IEvent
	//{
	//	public Guid AggId => Payload.AggId;
	//	public string Deco { get; set; }
	//}
}
