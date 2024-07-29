using Fuxion.Text.Json;

namespace Fuxion.Pods.Test.Text.Json;

public class JsonPodInlinePolymorphism : BaseTest<JsonPodInlinePolymorphism>
{
	public JsonPodInlinePolymorphism(ITestOutputHelper output) : base(output) => Printer.WriteLineAction = output.WriteLine;
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
			.ToJsonNode(nameof(Cat));
		var jsonText = pod.SerializeToJson();
		
		// Assert
		Output.WriteLine(jsonText);
	}
	[Fact]
	public void StringToPod_ShouldReturnRequestedObject_WhenStringIsCorrect()
	{
		var jsonString = """
								{
									"__discriminator": "Dog",
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

file class JsonNodePod<TInline> : Pod<string, TInline> { }

file abstract class Animal
{
	public string Name { get; set; } = null!;
	public int Age { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.Now;
}

file class Dog : Animal { }

file class Cat : Animal { }