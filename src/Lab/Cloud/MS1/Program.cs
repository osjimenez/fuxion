using System.Reactive.Linq;
using System.Text;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Lab.Cloud.MS1;
using Fuxion.Lab.Common;
using Fuxion.Reflection;
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
var rabbitSettings = builder.Configuration.GetSection("Rabbit").Get<RabbitSettings>()
							?? throw new InvalidProgramException($"Rabbit settings could not be loaded");
// builder.Services.AddSingleton<IRabbitMQPersistentConnection>(new DefaultRabbitMQPersistentConnection(new ConnectionFactory
// {
// 	HostName = rabbitSettings.Host, Port = rabbitSettings.Port
// }, 5));
// builder.Services.AddHostedService<RabbitHostedService>(sp => new("MS1-queue", sp.GetRequiredService<IRabbitMQPersistentConnection>()));

// MQTT
var mqttSettings = builder.Configuration.GetSection("Mqtt").Get<MqttSettings>()
						 ?? throw new InvalidProgramException($"Mqtt settings could not be loaded");
// builder.Services.AddHostedService<MqttHostedService>(sp => new(mqttSettings.Host, mqttSettings.Port, "MS1-topic", sp.GetRequiredService<ILogger<MqttHostedService>>()));

// NODE
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<RabbitMQSubscriber>();
// TODO Inject INexus as scoped
builder.Services.AddSingleton<INexus>(sp =>
{
	DefaultNexus nexus = new ("CL1-MS1");
	// var t = new RabbitMQRouteAdapter(), Receive);
	// var tt = new RabbitMQRoute(sp.GetRequiredService<IOptions<RabbitSettings>>(), sp.GetRequiredService<ILogger<RabbitMQRoute>>());
	var rabbitPublisher = sp.GetRequiredService<RabbitMQPublisher>();
	// nexus.RouteDirectory.AddPublisher<RabbitMQSend>(new(""), tt.Send);
	nexus.RouteDirectory.AddPublisher<TestMessage>(new(""), message => rabbitPublisher.Publish(new()
	{
		RoutingKey = "fuxion-lab-CL1-MS1",
		Body = Encoding.UTF8.GetBytes(message.ToJson())
	}));
	var rabbitSubscriber = sp.GetRequiredService<RabbitMQSubscriber>();
	nexus.RouteDirectory.AddSubscriber(rabbitSubscriber);
	new ObservableSubscriberDecorator<object>(rabbitSubscriber).Observe(_ => true)
		.Subscribe(msg =>
		{
			Console.WriteLine("Message observed:\r\n" + msg);
		});
	// rabbitSubscriber.OnReceive(msg =>
	// {
	// 	Console.WriteLine("Received ...");
	// });
	return nexus;
	// return new DefaultNexus("CL1-MS1", new DefaultProducer(), new DefaultConsumer(), new[]
	// {
	// 	new ObjectRouteAdapter<RabbitMQSend, RabbitMQReceive>(
	// 		new RabbitMQRouteAdapter(new RabbitMQRoute(sp.GetRequiredService<IOptions<RabbitSettings>>(), sp.GetRequiredService<ILogger<RabbitMQRoute>>()), Receive))
	// 	// new RabbitMQRoute(sp.GetRequiredService<IOptions<RabbitSettings>>(), sp.GetRequiredService<ILogger<RabbitMQRoute>>())
	// });
});

// APP
var app = builder.Build();
{
	var nexus = app.Services.GetRequiredService<INexus>();
	await nexus.Initialize();
	var rabbitConnection = app.Services.GetRequiredService<RabbitMQConnection>();
	var rabbitSubscriber = app.Services.GetRequiredService<RabbitMQSubscriber>();
	rabbitSubscriber.Attach(nexus);
	await rabbitConnection.Initialize();
	await rabbitSubscriber.Initialize();
	await nexus.OnReceive(msg =>
	{
		Console.WriteLine("Message received:\r\n" + msg);
	});
	nexus.Observe()
		.Buffer(2)
		.Subscribe(list =>
		{
			foreach(var msg in list)
				Console.WriteLine("Message observed from nexus extensions:\r\n" + msg);
		});
	new ObservableNexusDecorator(nexus).Observe()
		.Subscribe(msg =>
		{
			Console.WriteLine("Message observed from nexus:\r\n" + msg);
		});
}
// MAPS
app.MapGet("/instance", () => $"MS1 - Instance = {StaticInstance.Id}");
// app.MapGet("/send/rabbit", async (IRabbitMQPersistentConnection persistentConnection) => await RabbitHostedService.Send(persistentConnection, "MS2-queue", "Hola Mundo desde MS1"));
// app.MapGet("/send/rabbit", async (INexus nexus) => await nexus.Producer.Produce(new TestMessage(1,"test")));
app.MapGet("/send/rabbit", async (INexus nexus) => await nexus.Publish(new TestMessage(1,"test")));
app.MapGet("/send/mqtt", async () => await MqttHostedService.Publish("MS2-topic", "Hola Mundo desde MS1"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();