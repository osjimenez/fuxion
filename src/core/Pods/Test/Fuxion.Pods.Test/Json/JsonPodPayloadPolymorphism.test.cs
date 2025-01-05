using System.Text.Json;
using Fuxion.Pods.Json;
using Fuxion.Pods.Json.Serialization;
using Fuxion.Pods.Test.Compression;

namespace Fuxion.Pods.Test.Json;
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
		
		// Assert
		PrintVariable(pod.SerializeToJson(true));
	}
	[Fact]
	public void StringToPod_ShouldReturnRequestedObject_WhenStringIsCorrect()
	{
		var json = """
						{
							"__discriminator": "Dog",
							"__payload": {
								"name": "Firulais",
								"age": 3,
								"createdAt": "2018-02-28T00:00:00"
							}
						}
						""";

		// Act
		json.BuildPod()
			.FromJsonNode<string>(out var pod);
		var dog = pod.As<Dog>();
		
		// Assert
		PrintVariable(pod.SerializeToJson(true));
		Assert.Equal("Dog", pod.Discriminator);
		Assert.NotNull(dog);
		Assert.Equal("Firulais", dog.Name);
		Assert.Equal(3, dog.Age);
	}
}

file abstract class Animal
{
	public string Name { get; set; } = null!;
	public int Age { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.Now;
}

file class Dog : Animal { }

file class Cat : Animal { }