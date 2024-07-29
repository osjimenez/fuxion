using System.Text.Json;
using Fuxion.Text.Json;
using Fuxion.Text.Json.Serialization;

namespace Fuxion.Pods.Test.Text.Json;
// var jsonString = """
// 						{
// 							"__discriminator": "Dog",
// 							"name": "Firulais",
// 							"age": 3,
// 							"createdAt": "2018-02-28T00:00:00"
// 						}
// 						""";

// var jsonString = """
// 						{
// 							"Dog": {
// 								"name": "Firulais",
// 								"age": 3,
// 								"createdAt": "2018-02-28T00:00:00"
// 							}
// 						}
// 						""";
/// <summary>
///    There are different ways to implement polymorphism in JSON:
///    - Payload: The object is serialized with a property that contains the object.
///    - Inline: The object is serialized inline.
///    - Key: The type is used as the key of the object.
/// </summary>
public class JsonPodPayloadPolymorphism : BaseTest<JsonPodPayloadPolymorphism>
{
	public JsonPodPayloadPolymorphism(ITestOutputHelper output) : base(output) => Printer.WriteLineAction = output.WriteLine;
	[Fact]
	public void ObjectToStringPod_ShouldReturnCorrectString_WhenObjectIsCorrect()
	{
		// Arrange
		var cat = new Cat
		{
			Name = "Garfield",
			Age = 5
		};
		
		// Act
		var pod = cat.BuildPod()
			.ToJsonNode(nameof(Cat))
			.Pod;
		
		var options = new JsonSerializerOptions();
		options.Converters.Add(new IPodConverterFactory());
		var jsonText = pod.SerializeToJson(options: options);
		
		// Assert
		Output.WriteLine(jsonText);
	}
	[Fact]
	public void StringToPod_ShouldReturnRequestedObject_WhenStringIsCorrect()
	{
		var jsonString = """
								{
									"__discriminator": "Dog",
									"__payload": {
										"name": "Firulais",
										"age": 3,
										"createdAt": "2018-02-28T00:00:00"
								}
								""";
		
		// Act
		jsonString.BuildPod()
			.FromJsonNode<string>(out var pod);
		
		var dog = pod.As<Dog>();
		
		// Assert
		Output.WriteLine(pod.SerializeToJson());
		Assert.Equal("Dog", pod.Discriminator);
		Assert.Equal("Firulais", dog!.Name);
		Assert.Equal(3, dog!.Age);
	}
}

file class JsonNodePod<TPayload> : Pod<string, TPayload> { }

file abstract class Animal
{
	public string Name { get; set; } = null!;
	public int Age { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.Now;
}

file class Dog : Animal { }

file class Cat : Animal { }