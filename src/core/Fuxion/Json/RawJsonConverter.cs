using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Json
{
	public class RawJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
			=> writer.WriteRawValue(value?.ToString());
		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
			=> JToken.Load(reader).ToString(serializer.Formatting);
		public override bool CanConvert(Type objectType)
			=> typeof(string).IsAssignableFrom(objectType);
	}
}
