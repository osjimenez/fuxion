using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuxion.Reflection;
using PodType = Fuxion.IPod<string, string>;

namespace Fuxion.Text.Json.Serialization;

public class IPodConverterFactory(IUriKeyResolver? resolver = null) : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(IPod<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var podType = typeToConvert.GetSubclassOfRawGeneric(typeof(IPod<,>));
		var disType = podType?.GetProperty(nameof(PodType.Discriminator))
			?.PropertyType;
		var payType = podType?.GetProperty(nameof(PodType.Payload))
			?.PropertyType;
		if (disType is null || payType is null) throw new ApplicationException($"Cannot determine types for discriminator and payload of type '{typeToConvert.GetSignature()}'");
		var converterType = typeof(IPodConverter<,,>).MakeGenericType(typeToConvert, disType, payType);
		return (JsonConverter)(Activator.CreateInstance(converterType, resolver) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}