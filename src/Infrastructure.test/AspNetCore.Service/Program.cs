using System.Text.Json;
using System.Text.Json.Serialization;
using Fuxion.Text.Json.Serialization;

namespace Fuxion.AspNetCore.Service;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.

		// Configurar la serialización JSON
		builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
		{
			options.SerializerOptions.Converters.Add(new ExceptionConverter());
		});

		builder.Services.AddControllers();

		var app = builder.Build();

		// Configure the HTTP request pipeline.

		app.MapControllers();
		app.MapEndpointsForAssembly(typeof(Program).Assembly);

		app.Run();
	}
}