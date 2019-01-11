using Fuxion.Json;
using Fuxion.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
			var pod = payload.ToJsonPod("podKey");
			string json = pod.ToJson();

			Output.WriteLine("Serialized json: ");
			Output.WriteLine(json);

			Assert.Contains(@"""PayloadKey"": ""podKey""", json);
			Assert.Contains(@"""Name"": ""payloadName""", json);
			Assert.Contains(@"""Age"": 23", json);
			Assert.Contains(@"""Nick"": ""payloadNick""", json);
		}
		[Fact(DisplayName = "JsonPod - FromJson")]
		public void FromJson()
		{
			var json = @"
			{
				""PayloadKey"": ""podKey"",
				""Payload"": {
					""Name"": ""payloadName"",
					""Age"": 23,
					""Nick"": ""payloadNick""
				}
			}";

			Output.WriteLine("Initial json: ");
			Output.WriteLine(json);

			var pod = json.FromJsonPod<PayloadBase, string>();

			Output.WriteLine("pod.PayloadJRaw.Value: ");
			Output.WriteLine(pod.PayloadJRaw.Value.ToString());

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

			Assert.Equal("podKey", pod.PayloadKey);
			AssertBase(pod);
			Assert.Throws<InvalidCastException>(() => AssertDerived((PayloadDerived)pod));
			AssertDerived(pod.CastWithPayload<PayloadDerived>());
		}
		[Fact(DisplayName = "JsonPod - CastWithPayload")]
		public void CastWithPayload()
		{
			var payload = new PayloadDerived
			{
				Name = "payloadName",
				Age = 23,
				Nick = "payloadNick"
			};
			var basePod = new JsonPod<PayloadBase, string>(payload, "podKey");
			var derivedPod = basePod.CastWithPayload<PayloadDerived>();
			Assert.Equal("payloadName", derivedPod.Payload.Name);
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
}
