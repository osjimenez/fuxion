using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    //[JsonConverter(typeof(LicenseDataConverter))]
    //[JsonConverter(typeof(RawJsonConverter))]
    public class LicenseData
    {
        public LicenseData() { }
        public LicenseData(License content)
        {
            //var type = content.GetType();
            //Content = content;
            var json = content.ToJson(Formatting.None);
            Content = new JRaw(json);
        }
        //License _Content;
        //[JsonIgnore]
        //public LicenseContent Content { get { return _Content ?? (_Content = ContentAs<LicenseContent>()); } }

        //public string ContentType { get; set; }
        //[JsonProperty(PropertyName = nameof(Content))]
        //public JRaw ContentRaw
        //{
        //    get { return new JRaw(JsonConvert.SerializeObject(Content)); }
        //    set
        //    {
        //        var raw = value.ToString(Formatting.None);
        //        var type = Assembly.GetType(ContentType);
        //        Content = (LicenseContent)JsonConvert.DeserializeObject(raw, type);
        //    }
        //}






        public JRaw Content { get; set; }
        public T ContentAs<T>()
        {
            return Content.Value.ToString().FromJson<T>();
        }
        public bool ContentIs<T>()
        {
            try
            {
                Content.Value.ToString().FromJson<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    public class LicenseDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LicenseData);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var data = value as LicenseData;
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(LicenseData.Content));
            writer.WriteRawValue(data.Content.ToJson(Formatting.None));
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //return JObject.Load(reader).ToObject(objectType);

            var data = existingValue as LicenseData ?? new LicenseData(null);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject) continue;

                var value = reader.Value?.ToString();
                switch (value)
                {
                    //case nameof(LicenseData.ContentType):
                    //    data.ContentType = reader.ReadAsString();
                    //    break;
                    case nameof(LicenseData.Content):
                        var str = reader.ReadAsString();
                        //data.Content = new JRaw(str);
                        //data.Content = reader.Value as JRaw;
                        //data.ContentJson = reader.ReadAsString();
                        break;
                    default:
                        //product.fields.Add(value, reader.ReadAsString());
                        break;
                }

            }
            return data;
        }
    }

    //public class RawJsonConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return objectType == typeof(string);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        var sb = new StringBuilder();
    //        JsonToken previousToken = JsonToken.None;

    //        if (reader.TokenType == JsonToken.StartObject)
    //        {
    //            sb.Append('{');
    //            int depth = 1;
    //            while (depth > 0)
    //            {
    //                if (!reader.Read())
    //                    break;
    //                switch (reader.TokenType)
    //                {
    //                    case JsonToken.PropertyName:
    //                        if (previousToken == JsonToken.Boolean || previousToken == JsonToken.Integer || previousToken == JsonToken.Float)
    //                            sb.Append(',');
    //                        sb.AppendFormat("\"{0}\":", reader.Value);
    //                        break;
    //                    case JsonToken.StartArray:
    //                        if (previousToken == JsonToken.EndArray)
    //                            sb.Append(',');
    //                        sb.Append('[');
    //                        break;
    //                    case JsonToken.Boolean:
    //                    case JsonToken.Integer:
    //                    case JsonToken.Float:
    //                        if (previousToken == JsonToken.Boolean || previousToken == JsonToken.Integer || previousToken == JsonToken.Float)
    //                            sb.Append(',');
    //                        sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0}", reader.Value);
    //                        break;
    //                    case JsonToken.EndArray:
    //                        sb.Append(']');
    //                        break;
    //                    case JsonToken.StartObject:
    //                        sb.Append('{');
    //                        depth++;
    //                        break;
    //                    case JsonToken.EndObject:
    //                        sb.Append('}');
    //                        depth--;
    //                        break;
    //                }
    //                previousToken = reader.TokenType;
    //            }
    //        }
    //        return sb.ToString();
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        writer.WriteRawValue(value.ToString());
    //    }

    //    public override bool CanWrite
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }
    //}
}
