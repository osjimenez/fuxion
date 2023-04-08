using Fuxion.Lab.Cloud.MS1;
using Fuxion.Lab.Common;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RABBIT
builder.Configuration.AddJsonFile("rabbitsettings.json");
var rabbitSettings = builder.Configuration.GetSection("Rabbit").Get<RabbitSettings>()
							?? throw new InvalidProgramException($"Rabbit settings could not be loaded");
builder.Services.AddSingleton<IRabbitMQPersistentConnection>(new DefaultRabbitMQPersistentConnection(new ConnectionFactory
{
	HostName = rabbitSettings.Host, Port = rabbitSettings.Port
}, 5));
builder.Services.AddHostedService<RabbitHostedService>(sp => new("MS1-queue", sp.GetRequiredService<IRabbitMQPersistentConnection>()));

// MQTT
builder.Configuration.AddJsonFile("mqttsettings.json");
var mqttSettings = builder.Configuration.GetSection("Mqtt").Get<MqttSettings>()
						 ?? throw new InvalidProgramException($"Mqtt settings could not be loaded");
builder.Services.AddHostedService<MqttHostedService>(sp => new(mqttSettings.Host, mqttSettings.Port, "MS1-topic", sp.GetRequiredService<ILogger<MqttHostedService>>()));

// APP
var app = builder.Build();

// MAPS
app.MapGet("/instance", () => $"MS1 - Instance = {StaticInstance.Id}");
app.MapGet("/send/rabbit", async (IRabbitMQPersistentConnection persistentConnection) => await RabbitHostedService.Send(persistentConnection, "MS2-queue", "Hola Mundo desde MS1"));
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