using Fuxion.Domain;
using Fuxion.Lab.Cloud.MS1;
using Fuxion.Lab.Common;
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
// var rabbitSettings = builder.Configuration.GetSection("Rabbit").Get<RabbitSettings>()
// 							?? throw new InvalidProgramException($"Rabbit settings could not be loaded");
// builder.Services.AddSingleton<IRabbitMQPersistentConnection>(new DefaultRabbitMQPersistentConnection(new ConnectionFactory 
// {
// 	HostName = rabbitSettings.Host, Port = rabbitSettings.Port
// }, 5));
// builder.Services.AddHostedService<RabbitHostedService>(sp => new("MS2-queue", sp.GetRequiredService<IRabbitMQPersistentConnection>()));

// MQTT
// var mqttSettings = builder.Configuration.GetSection("Mqtt").Get<MqttSettings>()
// 						 ?? throw new InvalidProgramException($"Mqtt settings could not be loaded");
// builder.Services.AddHostedService<MqttHostedService>(sp => new(mqttSettings.Host, mqttSettings.Port, "MS2-topic", sp.GetRequiredService<ILogger<MqttHostedService>>()));

// NODE
// builder.Services.AddSingleton<INexus>(sp => new DefaultNexus("CL1-MS2",new DefaultProducer(), new DefaultConsumer(), new[]
// {
// 	new RabbitMQRoute(sp.GetRequiredService<IOptions<RabbitSettings>>(), sp.GetRequiredService<ILogger<RabbitMQRoute>>())
// }));

// APP
var app = builder.Build();

var node = app.Services.GetRequiredService<INexus>();
await node.Initialize();

// MAPS
app.MapGet("/instance", () => $"MS2 - Instance = {StaticInstance.Id}");
app.MapGet("/send/rabbit", async (IRabbitMQPersistentConnection persistentConnection) => await RabbitHostedService.Send(persistentConnection, "MS1-queue", "Hola Mundo desde MS2"));
app.MapGet("/send/mqtt", async () => await MqttHostedService.Publish("MS1-topic", "Hola Mundo desde MS2"));

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