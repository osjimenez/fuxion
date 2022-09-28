namespace Fuxion.Application.Test.Events;

using Fuxion.Application.Events;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Testing;
using System.Net.Mime;
using Xunit.Abstractions;

public class PublicationPodTest : BaseTest
{
	public PublicationPodTest(ITestOutputHelper output) : base(output) {
		Printer.WriteLineAction = m => output.WriteLine(m);
	}

	[Fact(DisplayName = "PublicationPodTest - ToJson")]
	public void ToJson()
	{
		var payload = new EventDerived(Guid.NewGuid())
		{
			Name = "payloadName",
			Age = 23,
			Nick = "payloadNick"
		};
		payload.AddEventSourcing(1, Guid.NewGuid(), DateTime.Now, 1);
		var pod = payload.ToEventSourcingPod();
		var json = pod.ToJson();

		Output.WriteLine("Serialized json: ");
		Output.WriteLine(json);

		Assert.Contains(@"""PayloadKey"":""" + nameof(EventDerived) + @"""", json);
		Assert.Contains(@"""Name"":""payloadName""", json);
		Assert.Contains(@"""Age"":23", json);
		Assert.Contains(@"""Nick"":""payloadNick""", json);
	}
	[Fact(DisplayName = "PublicationPodTest - FromJson")]
	public void FromJson()
	{
		var json = """
			{
				"TargetVersion": 1,
				"CorrelationId": "0260aeb2-7f6b-4f32-93e9-7202f7c14fdb",
				"EventCommittedTimestamp": "2022-09-27T11:24:24.0008666+02:00",
				"ClassVersion": 1,
				"PayloadKey": "EventDerived",
				"Payload": {
					"Nick": "payloadNick",
					"Name": "payloadName",
					"Age": 23,
					"AggregateId": "ad37c24b-76db-472a-a0fc-c3f68234c791"
				}
			}
			""";

		Output.WriteLine("Initial json: ");
		Output.WriteLine(json);

		var pod = json.FromEventSourcingPod();
		Assert.NotNull(pod);

		Output.WriteLine("pod.PayloadRaw.Value: ");
		Output.WriteLine(pod.PayloadRaw);

		void AssertBase(EventBase? payload)
		{
			Assert.NotNull(payload);
			Assert.Equal("payloadName", payload.Name);
			Assert.Equal(23, payload.Age);
		}
		void AssertDerived(EventDerived? payload)
		{
			Assert.NotNull(payload);
			AssertBase(payload);
			Assert.Equal("payloadNick", payload.Nick);
		}

		Assert.Equal(typeof(EventDerived).GetTypeKey(), pod.PayloadKey);
		AssertBase(pod.AsEvent<EventBase>());
		AssertDerived(pod.AsEvent<EventDerived>());
	}
}