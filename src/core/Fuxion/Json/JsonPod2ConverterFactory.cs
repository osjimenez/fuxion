﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

public class JsonPod2ConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPod2<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types = typeToConvert.GetGenericArguments();
		var converterType = typeof(JsonPod2Converter<,,>).MakeGenericType(typeof(JsonPod2<,>).MakeGenericType(types), types[0], types[1]);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}