using System.Reactive.Linq;
using System.Text;
using Fuxion;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Lab.Cloud.MS1;
using Fuxion.Lab.Common;
using Fuxion.Reflection;
using Fuxion.Text.Json;
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
builder.Services.AddSingleton<IUriKeyResolver, UriKeyDirectory>(_ =>
{
	UriKeyDirectory dir = new();
	dir.SystemRegister.All();
	dir.Register<TestMessage>();
	dir.Register<TestDestination>();
	return dir;
});

// NODE
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddScoped<RabbitMQPublisher>();
builder.Services.AddScoped<RabbitMQSubscriber>();
builder.Services.AddScoped<INexus>(sp =>
{
	DefaultNexus nexus = new ("CL1-MS1");
	var rabbitPublisher = sp.GetRequiredService<RabbitMQPublisher>();
	nexus.RouteDirectory.AddPublisher<IUriKeyPod<TestMessage>>(new(""), pod =>
	{
		var bytesPod = pod.RebuildUriKeyPod<TestMessage, IUriKeyPod<TestMessage>>()
			.ToJsonNode()
			.ToUtf8Bytes()
			.Pod;
		if (pod.TryGetHeader<TestDestination>(out var destination))
		{
			return rabbitPublisher.Publish(new()
			{
				RoutingKey = destination.Destination,
				Body = bytesPod.Payload
			});
		}
		throw new InvalidOperationException($"You must specify destination");
	});
	var rabbitSubscriber = sp.GetRequiredService<RabbitMQSubscriber>();
	var decoObs = new ObservableSubscriberDecorator<object>(rabbitSubscriber);
	decoObs.Observe(_ => true)
		.Subscribe(msg =>
		{
			Console.WriteLine("Message observed:\r\n" + msg);
		});
	nexus.RouteDirectory.AddSubscriber(decoObs);
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
// app.MapGet("/send/rabbit", () => {});
app.MapGet("/send/mqtt", async () => await MqttHostedService.Publish("MS2-topic", "Hola Mundo desde MS1"));

// RUN
app.Run();