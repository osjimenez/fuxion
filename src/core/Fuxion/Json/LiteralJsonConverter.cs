namespace Fuxion.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

public class LiteralJsonConverter : JsonConverter<string>
{
	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	  => reader.GetString();
	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
		writer.WriteRawValue(value);
}
