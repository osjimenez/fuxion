using System.Collections;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moq.Language.Flow;

namespace Fuxion.Test;

public class ClaimDemoBuffalo : BaseTest<ClaimDemoBuffalo>
{
	public ClaimDemoBuffalo(ITestOutputHelper output) : base(output) { }

	RequiredClaims GetClaims() => new()
	{
		RootItem = new Or()
		{
			Items = new()
			{
				new Claim("Rol", "Admin"),
				new And()
				{
					Items = new()
					{
						new Claim("Rol", "User"),
						new Claim("Town", "Valencia"),
					}
				}
			}
		}
		// {
		// 	Items = new()
		// 	{
		// 		new Claim("Rol", "Admin"),
		// 		new Or()
		// 		{
		// 			Items = new()
		// 			{
		// 				new Claim("Town", "Spain"),
		// 				new Claim("Town", "USA")
		// 			}
		// 		}
		// 	}
		// }
	};
	
	[Fact]
	public void Serialize()
	{
		var log = GetClaims();
		var json = log.SerializeToJson(true);
		Output.WriteLine(json);
		var res = json.DeserializeFromJson<RequiredClaims>(usePrivateConstructor:false);
	}
	[Fact]
	public void TryLogic()
	{
		var log = GetClaims();
		System.Security.Claims.Claim[] claims = new[]
		{
			new System.Security.Claims.Claim("Rol","User"),
			new System.Security.Claims.Claim("Town","Valencia")
		};
		var res = log.CheckItem(claims);
		Output.WriteLine("Res = "+res);
	}
	[Fact]
	public void Hash()
	{
		var input = """
			ss ClaimsLogicJsonConverter : JsonConverter<RequiredClaims>
			// {
			// 	public override RequiredClaims? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			// 	{
			// 		RequiredClaimsItem Process(ref Utf8JsonReader reader, string key)
			// 		{
			// 			if (key == "And")
			// 			{
			// 				reader.Read();
			// 				reader.Read();
			// 				reader.Read();
			// 				var k = reader.GetString();
			// 				return new And()
			""";
		using var ms = new MemoryStream(Encoding.Default.GetBytes(input));
		var al = MD5.Create();
		using var cs = new CryptoStream(ms, al, CryptoStreamMode.Read);
		var myFile = "_" + Path.GetRandomFileName();
		var writeStream = new MemoryStream();

		const int length = 16384;
		var buffer = new Byte[length];
		var bytesRead = cs.Read(buffer, 0, length);
		while (bytesRead > 0)
		{
			writeStream.Write(buffer, 0, bytesRead);
			bytesRead = cs.Read(buffer, 0, length);
		}
		cs.Flush();
		cs.Close();

		var res = Encoding.Default.GetString(writeStream.ToArray());
		Output.WriteLine("HASH = "+al.Hash?.ToHexadecimal());
		Output.WriteLine(res);

	}
}

// public class ClaimsLogicJsonConverter : JsonConverter<RequiredClaims>
// {
// 	public override RequiredClaims? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
// 	{
// 		RequiredClaimsItem Process(ref Utf8JsonReader reader, string key)
// 		{
// 			if (key == "And")
// 			{
// 				reader.Read();
// 				reader.Read();
// 				reader.Read();
// 				var k = reader.GetString();
// 				return new And()
// 				{
// 					
// 				};
// 			}else if (key == "Or")
// 			{
// 				
// 			}else if (key == "Claim")
// 			{
// 				
// 			}
// 		}
// 		RequiredClaims res = new();
// 		if (reader.TokenType == JsonTokenType.StartArray)
// 		{
// 			reader.Read();
// 			if (reader.TokenType == JsonTokenType.StartObject)
// 			{
// 				reader.Read();
// 				var key = reader.GetString();
// 				if (key is null) throw new JsonException();
// 				Process(ref reader, key);
// 			}
// 			return res;
// 		} else throw new JsonException();
// 	}
// 	public override void Write(Utf8JsonWriter writer, RequiredClaims value, JsonSerializerOptions options)
// 	{
// 		void Process(RequiredClaimsItem log)
// 		{
// 			writer.WriteStartObject();
// 			if (log is And and)
// 			{
// 				writer.WritePropertyName("And");
// 				writer.WriteStartArray();
// 				foreach(var l in and)
// 					Process(l);
// 				writer.WriteEndArray();
// 				
// 			}else if (log is Or or)
// 			{
// 				writer.WritePropertyName("Or");
// 				writer.WriteStartArray();
// 				foreach(var l in or)
// 					Process(l);
// 				writer.WriteEndArray();
// 			}else if (log is Claim claim)
// 			{
// 				writer.WriteString("Key",claim.Key);
// 				writer.WriteString("Value",claim.Value);
// 			} else throw new JsonException("Type not expected");
// 			writer.WriteEndObject();
// 			
// 		}
// 		writer.WriteStartArray();
// 		foreach(var log in value.Logics)
// 		{
// 			Process(log);
// 		}
// 		writer.WriteEndArray();
// 	}
// }
// [JsonConverter(typeof(ClaimsLogicJsonConverter))]
public class RequiredClaims
{
	public RequiredClaimsItem? RootItem { get; set; }
	public bool CheckIdentity(ClaimsIdentity claimsIdentity)
		=> CheckItem(claimsIdentity.Claims.ToArray());
	public bool CheckItem(params System.Security.Claims.Claim[] claims) 
		=> RootItem is not null && CheckItem(RootItem, claims);
	bool CheckItem(RequiredClaimsItem item, params System.Security.Claims.Claim[] claims)
		=> item switch
		{
			And and => and.Items.All(i => CheckItem(i, claims)),
			Or or => or.Items.Any(i => CheckItem(i, claims)),
			Claim claim => claims.Any(c => c.Type == claim.Key && c.Value == claim.Value),
			_ => false
		};
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Claim), typeDiscriminator: "Claim")]
[JsonDerivedType(typeof(And), typeDiscriminator: "And")]
[JsonDerivedType(typeof(Or), typeDiscriminator: "Or")]
public abstract class RequiredClaimsItem { }
public class Claim : RequiredClaimsItem
{
	public Claim(string key, string value)
	{
		Key = key;
		Value = value;
	}
	public string Key { get; set; }
	public string Value { get; set; }
}
public class And : RequiredClaimsItem
{
	public List<RequiredClaimsItem> Items { get; set; } = new();
}
public class Or : RequiredClaimsItem
{
	public List<RequiredClaimsItem> Items { get; set; } = new();
}