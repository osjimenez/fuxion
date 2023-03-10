using Fuxion.Application.Events;
using Fuxion.Reflection;
using Fuxion.Testing;
using Xunit.Abstractions;

namespace Fuxion.Application.Test.Events;

public class PublicationPodTest : BaseTest<PublicationPodTest>
{
	public PublicationPodTest(ITestOutputHelper output) : base(output) => Printer.WriteLineAction = m => output.WriteLine(m);
	[Fact(DisplayName = "PublicationPodTest - FromJson")]
	public void FromJson()
	{
		var json = $$"""
			{
				"TargetVersion": 1,
				"CorrelationId": "0260aeb2-7f6b-4f32-93e9-7202f7c14fdb",
				"EventCommittedTimestamp": "2022-09-27T11:24:24.0008666+02:00",
				"ClassVersion": 1,
				"Discriminator": "Fuxion/Application/Test/Events/DerivedEvent",
				"Payload": {
					"Nick": "payloadNick",
					"Name": "payloadName",
					"Age": 23,
					"AggregateId": "ad37c24b-76db-472a-a0fc-c3f68234c791"
				}
			}
			""";
		// Output.WriteLine("Initial json: ");
		// Output.WriteLine(json);
		// var pod = json.FromEventSourcingPod();
		// Assert.NotNull(pod);
		// Output.WriteLine("pod.PayloadValue: ");
		// Output.WriteLine(pod.PayloadValue.ToString());
		// void AssertBase(BaseEvent? payload)
		// {
		// 	Assert.NotNull(payload);
		// 	Assert.Equal("payloadName", payload.Name);
		// 	Assert.Equal(23, payload.Age);
		// }
		// void AssertDerived(DerivedEvent? payload)
		// {
		// 	Assert.NotNull(payload);
		// 	AssertBase(payload);
		// 	Assert.Equal("payloadNick", payload.Nick);
		// }
		// Assert.Equal(typeof(DerivedEvent).GetTypeKey(), pod.Discriminator);
		// AssertBase(pod.AsEvent<BaseEvent>());
		// AssertDerived(pod.AsEvent<DerivedEvent>());
	}
	[Fact(DisplayName = "PublicationPodTest - ToJson")]
	public void ToJson()
	{
		// var payload = new DerivedEvent(Guid.NewGuid(), "payloadName", 23, "payloadNick");
		// payload.AddEventSourcing(1, Guid.NewGuid(), DateTime.Now, 1);
		// var pod = payload.ToEventSourcingPod();
		// var json = pod.ToJson();
		// Output.WriteLine("Serialized json: ");
		// Output.WriteLine(json);
		// Assert.Contains($@"""Discriminator"": ""Fuxion/Application/Test/Events/DerivedEvent""", json);
		// Assert.Contains(@"""Name"": ""payloadName""", json);
		// Assert.Contains(@"""Age"": 23", json);
		// Assert.Contains(@"""Nick"": ""payloadNick""", json);
	}
}