using Fuxion;
using Fuxion.AspNetCore;
using Fuxion.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace AspNetCore.Test;

public class ResponseTest(ITestOutputHelper output) : BaseTest<ResponseTest>(output)
{
	public Response GetSuccess() => Response.Get.Success();
	public Response GetSuccessWithPayload() => Response.Get.Success(123);
	public Response GetError() => Response.Get.Error("message4");
	public Response GetErrorWithPayload() => Response.Get.Error("message", 123);
	public Response GetNotFound() => Response.Get.Error.NotFound("message");
	public Response GetNotFoundWithPayload() => Response.Get.Error.NotFound("message", 123);
	[Fact]
	public void ToApiResult()
	{
		PrintVariable(GetSuccess().ToApiResult().SerializeToJson(true));
		PrintVariable(GetSuccessWithPayload().ToApiResult().SerializeToJson(true));
	}
}
