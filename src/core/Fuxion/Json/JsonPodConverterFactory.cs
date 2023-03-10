using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

public class JsonPodConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPod<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types = typeToConvert.GetGenericArguments();
		// var converterVars = types.Length switch {
		// 	2 => new {
		// 		GenericArguments = new[] { typeof(JsonPod<,>).MakeGenericType(types), types[0], types[1] },
		// 		ConverterGenericType = typeof(JsonPodConverter<,,>)
		// 	},
		// 	3 => new {
		// 		GenericArguments =new[] { typeof(JsonPod<,,>).MakeGenericType(types), types[0], types[1], types[2] },
		// 		ConverterGenericType = typeof(JsonPodConverter<,,,>)
		// 	},
		// 	_ => throw new InvalidProgramException($"'{typeToConvert.GetSignature()}' generic arguments expected to be 2 or 3.")
		// };
		// var converterType = converterVars.ConverterGenericType.MakeGenericType(converterVars.GenericArguments);
		var converterType = typeof(JsonPodConverter<,,>).MakeGenericType(typeof(JsonPod<,>).MakeGenericType(types), types[0], types[1]);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}