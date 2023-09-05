using Fuxion.Application.Commands;
using Fuxion.Application.Events;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Xunit;
using Xunit.Abstractions;

namespace Fuxion.Application.Test.Commands;

public class CommandPodTest : BaseTest<CommandPodTest>
{
	public CommandPodTest(ITestOutputHelper output) : base(output) { }
// 	[Fact(DisplayName = "CommandPodTest - FromJson")]
// 	public void FromJson()
// 	{
// 		var tkd = new TypeKeyDirectory();
// 		tkd.Register<BaseCommand>();
// 		var id = Guid.Parse("52d307a2-39ba-47e2-b73f-2faf0727d44e");
// 		var json = $$"""
// 		{
// 			"Discriminator": "Fuxion/Application/Test/Commands/BaseCommand",
// 			"Payload": {
// 				"Name": "mockName",
// 				"AggregateId": "52d307a2-39ba-47e2-b73f-2faf0727d44e"
// 			}
// 		}
// 		""";
// 		var pod = json.FromJson<FeaturizablePod<Command>>();
// 		Output.WriteLine(json);
// 		Assert.NotNull(pod);
// 		Output.WriteLine(pod.Discriminator.ToString());
//
// 		Assert.Equal(new [] { nameof(Fuxion),nameof(Application),nameof(Test),nameof(Commands), nameof(BaseCommand) }, pod.Discriminator);
// 		Assert.False(pod.PayloadHasValue);
// 		Assert.Null(pod.Payload);
// 		var com = (Command?)pod.As(tkd[pod.Discriminator]);
// 		// var com = pod.WithTypeKeyResolver(tkd);
// 		Assert.Equal(id, com?.AggregateId);
// 		Assert.IsType<BaseCommand>(com);
// 		var mevt = (BaseCommand?)com;
// 		Assert.Equal("mockName", mevt?.Name);
// 	}
// 	[Fact(DisplayName = "CommandPodTest - ToJson")]
// 	public void ToJson()
// 	{
// 		var id = Guid.NewGuid();
// 		var com = new BaseCommand(id, "mockName");
// 		var pod = new FeaturizablePod<Command>(com);
// 		// var pod = com.ToCommandPod();
// 		Assert.True(pod.PayloadHasValue);
// 		Assert.IsType<BaseCommand>(pod.Payload);
// 		var json = pod.ToJson();
// 		Output.WriteLine("Serialized json:");
// 		Output.WriteLine(json);
// 		Assert.Contains($@"""Discriminator"": ""Fuxion/Application/Test/Commands/BaseCommand"",", json);
// 		Assert.Contains($@"""AggregateId"": ""{id}""", json);
// 		Assert.Contains(@"""Name"": ""mockName""", json);
// 	}
}