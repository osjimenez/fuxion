using Fuxion.Application.Commands;
using Fuxion.Reflection;
using Fuxion.Testing;
using Xunit.Abstractions;

namespace Fuxion.Application.Test.Commands;

public class CommandPodTest : BaseTest<CommandPodTest>
{
	public CommandPodTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "CommandPodTest - FromJson")]
	public void FromJson()
	{
		var tkd = new TypeKeyDirectory();
		tkd.Register<BaseCommand>();
		var id = Guid.Parse("52d307a2-39ba-47e2-b73f-2faf0727d44e");
		var json = $$"""
		{
			"PayloadKey": "{{nameof(BaseCommand)}}",
			"Payload": {
				"Name": "mockName",
				"Id": "52d307a2-39ba-47e2-b73f-2faf0727d44e"
			}
		}
		""";
		var pod = json.FromJson<CommandPod>();
		Output.WriteLine(json);
		Assert.NotNull(pod);
		Output.WriteLine(pod.PayloadKey);
		Assert.Equal(nameof(BaseCommand), pod.PayloadKey);
		Assert.False(pod.PayloadHasValue);
		Assert.Null(pod.Payload);
		var com = pod.WithTypeKeyDirectory(tkd);
		Assert.Equal(id, com?.Id);
		Assert.IsType<BaseCommand>(com);
		var mevt = (BaseCommand?)com;
		Assert.Equal("mockName", mevt?.Name);
	}
	[Fact(DisplayName = "CommandPodTest - ToJson")]
	public void ToJson()
	{
		var id  = Guid.NewGuid();
		var com = new BaseCommand(id, "mockName");
		var pod = com.ToCommandPod();
		Assert.True(pod.PayloadHasValue);
		Assert.IsType<BaseCommand>(pod.Payload);
		var json = pod.ToJson();
		Output.WriteLine("Serialized json:");
		Output.WriteLine(json);
		Assert.Contains($@"""PayloadKey"": ""{nameof(BaseCommand)}""", json);
		Assert.Contains($@"""Id"": ""{id}""",                          json);
		Assert.Contains(@"""Name"": ""mockName""",                     json);
	}
}
