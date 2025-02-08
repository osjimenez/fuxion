using System.Net;
using System.Text.Json;
using Fuxion.AspNetCore.Service;
using Fuxion.Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.AspNetCore.Test;

public class ResponseTest(ITestOutputHelper output, WebApplicationFactory<Program> factory) : BaseTest<ResponseTest>(output), IClassFixture<WebApplicationFactory<Program>>
{
	async Task Do(string prefix)
	{
		var cli = factory.CreateClient();
		var camelCaseOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		// SUCCESS
		var res = await cli.GetAsync($"{prefix}test-empty-success");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);

		res = await cli.GetAsync($"{prefix}test-message-success");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
		Assert.Equal("Success message", await res.Content.ReadAsStringAsync());

		res = await cli.GetAsync($"{prefix}test-payload-success");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
		var str = await res.Content.ReadAsStringAsync();
		var payload = str.DeserializeFromJson<TestPayload>(options: camelCaseOptions);
		Assert.Equal("Test name", payload?.FirstName);
		Assert.Equal(123, payload?.Age);

		// ERROR
		res = await cli.GetAsync($"{prefix}test-message-error");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
		str = await res.Content.ReadAsStringAsync();
		var problem = str.DeserializeFromJson<ProblemDetails>(options: camelCaseOptions);
		Assert.Equal("Error message", problem?.Detail);

		res = await cli.GetAsync($"{prefix}test-payload-error");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
		str = await res.Content.ReadAsStringAsync();
		problem = str.DeserializeFromJson<ProblemDetails>(options: camelCaseOptions);
		Assert.Equal("Error message", problem?.Detail);
		payload = ((JsonElement)problem?.Extensions["payload"]!).Deserialize<TestPayload>(camelCaseOptions);
		Assert.Equal("Test name", payload?.FirstName);
		Assert.Equal(123, payload?.Age);

		// BAD REQUEST
		res = await cli.GetAsync($"{prefix}test-message-bad-request");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
		str = await res.Content.ReadAsStringAsync();
		problem = str.DeserializeFromJson<ProblemDetails>(options: camelCaseOptions);
		Assert.Equal("Error message", problem?.Detail);

		res = await cli.GetAsync($"{prefix}test-payload-bad-request");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
		str = await res.Content.ReadAsStringAsync();
		problem = str.DeserializeFromJson<ProblemDetails>(options: camelCaseOptions);
		Assert.Equal("Error message", problem?.Detail);
		payload = ((JsonElement)problem?.Extensions["payload"]!).Deserialize<TestPayload>(camelCaseOptions);
		Assert.Equal("Test name", payload?.FirstName);
		Assert.Equal(123, payload?.Age);

		// EXCEPTION
		res = await cli.GetAsync($"{prefix}test-message-exception");
		PrintVariable(res.StatusCode);
		Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
		str = await res.Content.ReadAsStringAsync();
		PrintVariable(str);
		problem = str.DeserializeFromJson<ProblemDetails>(options: camelCaseOptions);
		var exception = (JsonElement)problem?.Extensions["exception"]!;
		Assert.Equal("This method was not implemented jet", exception.GetProperty("Message").GetString());
	}
	[Fact]
	public async Task ToApiActionResult() => await Do("controller/");
	[Fact]
	public async Task ToApiResult() => await Do("endpoint-");
}