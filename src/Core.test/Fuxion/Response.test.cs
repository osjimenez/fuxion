namespace Fuxion.Test;

public class ResponseTest(ITestOutputHelper output) : BaseTest<ResponseTest>(output)
{
	public Response GetSuccess() => Response.Get.Success();
	public Response GetSuccessMessage() => Response.Get.Success("message");
	public Response GetSuccessMessageWithExtensions() => Response.Get.Success("message", ("Extension", 123.456));
	public Response GetSuccessWithPayload() => Response.Get.Success(123);
	public Response GetSuccessWithPayloadAndExtensions()
		=> Response.Get.Success(123, extensions: ("Extension", 123.456));
	public Response GetError() => Response.Get.Error("message");
	public Response GetErrorWithPayload() => Response.Get.Error("message", 123);
	public Response GetNotFound() => Response.Get.Error.NotFound("message");
	public Response GetNotFoundWithPayload() => Response.Get.Error.NotFound("message", 123);
	public Response GetNotFoundWithPayloadAndExtensions() => Response.Get.Error.NotFound("message", 123, extensions: ("Extension", 123.456));
	public Response GetCustomError() => Response.Get.Custom("message", "customData");
	[Fact]
	public void ImplicitConversion()
	{
		Assert.Equal(123, OkInt());
		Assert.Equal(456, ErrorInt());
		int val = OkResponse();
		Assert.Equal(123, val);

		return;
		int OkInt() => Response.Get.Success(123);
		int ErrorInt() => Response.Get.Error("message", 456);
		Response<int> OkResponse() => 123;
	}
	[Fact]
	public void Success()
	{
		var s1 = Response.Get.Success();
		Assert.Null(s1.Message);
		Assert.Null(s1.Payload);
		var s2 = Response.Get.Success("message");
		Assert.NotNull(s2.Message);
		Assert.Null(s2.Payload);
		var s3 = Response.Get.Success(payload: "payload");
		Assert.Null(s3.Message);
		Assert.NotNull(s3.Payload);
		var s4 = Response.Get.Success(123);
		Assert.Null(s4.Message);
		Assert.Equal(123, s4.Payload);
	}
	[Fact]
	public void Serialize()
	{
		PrintVariable(GetSuccess().SerializeToJson(true));
		PrintVariable(GetSuccessMessage().SerializeToJson(true));
		PrintVariable(GetSuccessMessageWithExtensions().SerializeToJson(true));
		PrintVariable(GetSuccessWithPayload().SerializeToJson(true));
		PrintVariable(GetSuccessWithPayloadAndExtensions().SerializeToJson(true));

		PrintVariable(GetError().SerializeToJson(true));
		PrintVariable(GetErrorWithPayload().SerializeToJson(true));

		PrintVariable(GetNotFound().SerializeToJson(true));
		PrintVariable(GetNotFoundWithPayload().SerializeToJson(true));
		PrintVariable(GetNotFoundWithPayloadAndExtensions().SerializeToJson(true));

		PrintVariable(GetCustomError().SerializeToJson(true));

		var results = new Response[]
		{
			Response.Get.Success(),
			Response.Get.Success("message",("Extension", 123.456)),
			Response.Get.Success(123, "message"),
			Response.Get.Error.NotFound("message"),
			Response.Get.Error("message", new Payload("Bob", 25), extensions: ("Extension", 123.456))
		};
		PrintVariable(results.SerializeToJson(true));
		PrintVariable(results.CombineResponses().SerializeToJson(true));

		IsTrue(GetNotFound().IsNotFound());
		IsTrue(GetNotFound().IsErrorType(ErrorType.NotFound));
		IsTrue(GetNotFoundWithPayload().IsNotFound());
	}
	[Fact]
	public void Exception()
	{
		Dictionary<int, int> dic = new();
		var res = Do();
		PrintVariable(res.SerializeToJson(true));
		return;
		Response<int> Do()
		{
			try
			{
				return Do2();
			} catch (Exception ex)
			{
				PrintVariable(ex.SerializeToJson(true));
				return Response.Get.Error.Critical("Exception", exception: ex);
			}
		}
		Response<int> Do2()
		{
			return dic[1];
		}
	}
}

file record Payload(string Name, int Age);

file class CustomError(string message, string customData) : Response(false, message)
{
	public string CustomData { get; } = customData;
}

file static class CustomErrorExtensions
{
	public static CustomError Custom(this IResponseFactory _, string message, string customData) => new(message, customData);
}