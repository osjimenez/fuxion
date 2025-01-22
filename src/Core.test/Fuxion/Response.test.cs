namespace Fuxion.Test;

public class ResponseTest(ITestOutputHelper output) : BaseTest<ResponseTest>(output)
{
	public IResponse GetSuccess() => Response.Get.Success();
	public IResponse GetSuccessWithPayload() => Response.Get.Success(123);
	public IResponse GetError() => Response.Get.Error("message4");
	public IResponse GetErrorWithPayload() => Response.Get.Error("message", 123);
	public IResponse GetNotFound() => Response.Get.NotFound();
	public IResponse GetNotFoundWithPayload() => Response.Get.NotFound(123);
	public IResponse GetCustomError() => Response.Get.Custom("message", "customData");
	[Fact]
	public void ImplicitConversion()
	{
		Assert.Equal(123, Ok());
		Assert.Equal(456, Error());

		return;
		int Ok() => Response.Get.Success(123);
		int Error() => Response.Get.Error("", 456);
	}
	[Fact]
	public void Serialize()
	{
		PrintVariable(GetSuccess()
			.SerializeToJson(true));
		PrintVariable(GetSuccessWithPayload()
			.SerializeToJson(true));

		PrintVariable(GetError()
			.SerializeToJson(true));
		PrintVariable(GetErrorWithPayload()
			.SerializeToJson(true));

		PrintVariable(GetNotFound()
			.SerializeToJson(true));
		PrintVariable(GetNotFoundWithPayload()
			.SerializeToJson(true));

		PrintVariable(GetCustomError()
			.SerializeToJson(true));

		var results = new IResponse[]
		{
			Response.Get.Success(), Response.Get.Success("Data")
		};
		PrintVariable(results.CombineResponses()
			.SerializeToJson(true));

		IsTrue(GetNotFound()
			.IsNotFound());
		IsTrue(GetNotFound()
			.IsErrorType(ErrorType.NotFound));
		IsTrue(GetNotFoundWithPayload()
			.IsNotFound());
	}
}

public class CustomError(string message, string customData) : ErrorResponse(message)
{
	public string CustomData { get; } = customData;
}

public static class CustomErrorExtensions
{
	public static CustomError Custom(this IResponseFactory _, string message, string customData) => new(message, customData);
}