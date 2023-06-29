using System.Text.Json;
using System.Text.Json.Serialization;
using PodType = Fuxion.IPod<string, string>;

namespace Fuxion.Text.Json;

public class IPodConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(IPod<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var disType = typeToConvert.GetProperty(nameof(PodType.Discriminator))
			?.PropertyType;
		var payType = typeToConvert.GetProperty(nameof(PodType.Payload))
			?.PropertyType;
		if (disType is null || payType is null) throw new ApplicationException($"Cannot determine types for discriminator and payload of type '{typeToConvert.Name}'");
		var converterType = typeof(IPodConverter<,,>).MakeGenericType(typeToConvert, disType, payType);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}