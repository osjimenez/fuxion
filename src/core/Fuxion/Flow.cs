using System.Runtime.InteropServices.Marshalling;
using System.Security.Authentication;

namespace Fuxion;

// Workflow core
// https://github.com/danielgerlag/workflow-core
// https://workflow-core.readthedocs.io/en/latest/control-structures/
public class IFlow
{
	
}

public class a
{
	public void b()
	{
		// var commandFlow = CreateFlow<IUriKeyPod<Command>>()
		// 	.Handle(CommandHandle)
		//
		// var rountingFlow = CreateFlow<IUriKeyPod<object>>()
		// 	.IfTytpeIs<CreateSaltoUserComand>("SALTO-SERVICE")
		// 	.IfTytpeIs<Create2NUserComand>("2N-SERVICE")
		// 	
		// var destinationByTypeFlow = 
		// 	
		// var mainFlow = CreateFlow<object>()
		// 	.Protocols()
		// 	.Policies()
		// 	.Transform<ObjectToJsonElement>()
		// 	.Transform<JsonElementToJsonNode>()
		// 	.Transform<JsonNodeToUriKeyPod<object>>(out var pod)
		// 	.If(pod.TryGetHeader<Destination>(out var detinationHeader))
		// 		.Then(routingFlow)
		// 		.Else(destinationByTypeFlow)
	}
}