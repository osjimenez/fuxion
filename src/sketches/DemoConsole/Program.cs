namespace DemoConsole;

using Fuxion.Json;

internal class Program
{
	static void Main(string[] args)
	{
		var payload = new PayloadDerived
		{
			Name = "payloadName",
			Age = 23,
			Nick = "payloadNick"
		};
		var pod = payload.ToJsonPod("podKey");
		var json = pod.ToJson();

		Console.WriteLine("Serialized json: ");
		Console.WriteLine(json);

		//Assert.Contains(@"""PayloadKey"":""podKey""", json);
		//Assert.Contains(@"""Name"": ""payloadName""", json);
		//Assert.Contains(@"""Age"": 23", json);
		//Assert.Contains(@"""Nick"": ""payloadNick""", json);
	}
}
public class PayloadBase
{
	public string? Name { get; set; }
	public int Age { get; set; }
}
public class PayloadDerived : PayloadBase
{
	public string? Nick { get; set; }
}
