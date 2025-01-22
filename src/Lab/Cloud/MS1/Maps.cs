using System.Text.Json.Nodes;
using Fuxion.Domain;
using Fuxion.Lab.Common;
using Fuxion.Pods;

namespace Fuxion.Lab.Cloud.MS1;

public static class Maps
{
	public static void MapEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapGet("/instance", () => $"MS1 - Instance = {StaticInstance.Id}");
		app.MapGet("/send/rabbit", async (INexus nexus, IUriKeyResolver resolver) =>
		{
			TestMessage msg = new(1, "test");
			var pod = msg.BuildUriKeyPod(resolver)
				.ToUriKeyPod()
				.AddUriKeyHeader(new TestDestination("fuxion-lab-CL1-MS1"))
				.Pod;
			await nexus.Publish(pod);
		});
		//app.MapPost("/send", async (INexus nexus, UriKeyPod<object> pod) => await nexus.Publish(pod));
		//app.MapPost("/send", async (INexus nexus, JsonNode json) => await nexus.Publish(json));
		app.MapPost("/send", async (INexus nexus, object obj) => await nexus.Publish(obj));
	}
}