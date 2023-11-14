using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fuxion;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Lab.Cloud.MS1;
using Fuxion.Lab.Common;
using Fuxion.Reflection;
using Fuxion.Text.Json;
using Fuxion.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CONFIGURATION
builder.Configuration.AddJsonFile("rabbitsettings.json");
builder.Configuration.AddJsonFile("mqttsettings.json");
builder.Services.Configure<RabbitSettings>(builder.Configuration.GetSection("Rabbit"));
builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("Mqtt"));

// RABBIT
// var rabbitSettings = builder.Configuration.GetSection("Rabbit")
// 	.Get<RabbitSettings>() ?? throw new InvalidProgramException($"Rabbit settings could not be loaded");

// MQTT
// var mqttSettings = builder.Configuration.GetSection("Mqtt")
// 	.Get<MqttSettings>() ?? throw new InvalidProgramException($"Mqtt settings could not be loaded");

// DIRECTORY
UriKeyDirectory directory = new();
directory.SystemRegister.All();
directory.Register<TestMessage>();
directory.Register<TestDestination>();
builder.Services.AddSingleton<IUriKeyResolver>(directory);

// JSON
builder.Services.Configure<JsonOptions>(options =>
{
	options.SerializerOptions.Converters.Add(new IPodConverterFactory(directory));
});

// NODE
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddScoped<RabbitMQPublisher>();
builder.Services.AddScoped<RabbitMQSubscriber>();
builder.Services.AddScoped<INexus>(sp =>
{
	DefaultNexus nexus = new ("CL1-MS1");
	nexus.RouteDirectory.AddPublisher<object>(new(""), obj => obj is JsonElement, async obj => await nexus.Publish((JsonElement)obj));
	nexus.RouteDirectory.AddPublisher<JsonElement>(new(""), jsonElement => jsonElement.ValueKind == JsonValueKind.Object,
		async jsonElement => await nexus.Publish(JsonNode.Parse(jsonElement.GetRawText()) ?? throw new SerializationException($"The text couldn't be deserialized as JsonNode")));
	nexus.RouteDirectory.AddPublisher<JsonNode>(new(""), async jsonNode => await nexus.Publish(jsonNode.ToJsonString()
		.BuildUriKeyPod(directory)
		.FromJsonNode()
		.Pod));
	
	// TODO El orden de los dos siguientes publishers es importante, y eso no mola
	// Primero proceso los que tienen cabecera, luego los que no
	// Si le doy la vuelta, al procesar los que no tienen cabecera, modifico el pod, le agrego la cabecera y luego se procesa con cabecera.
	// como al agregar la cabecera lo vuelvo a publicar en el nexo, se envia dos veces
	
	// TODO Todo lo que llega no se tiene que enviar a rabbit, deberia haber un RoutePublisher o algo asi
	// que decida por donde se envia en funcion del destino
	nexus.RouteDirectory.AddPublisher<IUriKeyPod<object>>(new(""), pod => pod.TryGetHeader<TestDestination>(out var _), async pod =>
	{
		var bytesPod = pod.RebuildUriKeyPod<object, IUriKeyPod<object>>()
			.ToJsonNode()
			.ToUtf8Bytes()
			.Pod;
		if (pod.TryGetHeader<TestDestination>(out var destination))
		{
			// TODO aqui se decide, en funcion del nombre de destino, que se enviará por Rabbit
			// Esto deberia mejorarse
			if (destination.Destination == "fuxion-lab-CL1-MS1")
			{
				await nexus.Publish(new RabbitMQSend
				{
					RoutingKey = destination.Destination,
					Body = bytesPod.Payload
				});
			}
		}
		throw new InvalidOperationException($"You must specify destination");
	});
	nexus.RouteDirectory.AddPublisher<IUriKeyPod<object>>(new(""),pod => !pod.TryGetHeader<TestDestination>(out var _), async pod =>
	{
		// TODO Aqui es donde se decide la ruta/destino del mensaje
		// En este caso, simplemente si es del tipo TestMessage se envia a CL1-MS1
		// Hay que hacer un mecanismo de enrutado configurable y más claro
		// TODO https://medium.com/@kunaltandon.kt/boxing-and-unboxing-as-and-is-operator-in-c-4f0b4b4e1f32
		// Ahi dice (al final del articulo) que el operador 'is' hace dos unbox y el 'as' hace uno y es mejor
		if (pod.Payload is TestMessage)
		{
			var newPod = pod.RebuildUriKeyPod<object, IUriKeyPod<object>>()
				.AddUriKeyHeader(new TestDestination("fuxion-lab-CL1-MS1"))
				.Pod;
			await nexus.Publish(newPod);
		}
		throw new InvalidProgramException($"Destination couldn't be determined");
	});
	
	nexus.RouteDirectory.AddPublisher(sp.GetRequiredService<RabbitMQPublisher>());
	
	var rabbitSubscriber = sp.GetRequiredService<RabbitMQSubscriber>();
	var decoObs = new ObservableSubscriberDecorator<object>(rabbitSubscriber);
	// decoObs.Observe(_ => true)
	// 	.Subscribe(msg =>
	// 	{
	// 		Console.WriteLine("Message observed:\r\n" + msg);
	// 	});
	nexus.AddSubscriber(decoObs);
	return nexus;
});

// SERVICE
builder.Services.AddHostedService<Service>();

// APP
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// MAPS
app.MapEndpoints();

// RUN
app.Run();