using System.Text;
using System.Web;
using Fuxion.Reflection;

namespace Fuxion.Test;

public class UriKeyTest(ITestOutputHelper output) : BaseTest<UriKeyTest>(output)
{
	void AnalyseUriKey(UriKey uriKey, string title = "=== Analysing UriKey ===")
	{
		using (Printer.Indent(title))
		{
			Printer.WriteLine($"KEY: {uriKey.Key}");
			Printer.WriteLine($"FULL-URI: {uriKey.FullUri}");
			if (uriKey.Generics.Length == 0) goto interfaces;
			using (Printer.Indent("GENERICS"))
				for (var i = 0; i < uriKey.Generics.Length; i++)
				{
					var gen = uriKey.Generics[i];
					if (gen is null)
						Printer.WriteLine($"GENERIC_{i + 1} : UNDEFINED");
					else
						AnalyseUriKey(new(gen, true), $"GENERIC_{i + 1}");
				}
		interfaces:
			if (uriKey.Interfaces.Length == 0) goto bases;
			using (Printer.Indent("INTERFACES"))
				for (var i = 0; i < uriKey.Interfaces.Length; i++)
					AnalyseUriKey(new(uriKey.Interfaces[i], true), "INTERFACE");
		bases:
			if (uriKey.Bases.Length == 0) return;
			using (Printer.Indent("BASES"))
				for (var i = 0; i < uriKey.Bases.Length; i++)
					AnalyseUriKey(new(uriKey.Bases[i], true), $"BASE_{i + 1}");
		}
	}
	[Fact(DisplayName = "Base64 Url encoding")]
	public void Base64UrlEncoding()
	{
		var uriKey = typeof(Generic<,>).GetUriKey();
		AnalyseUriKey(uriKey);
		
		// var url = $"https://meta.fuxion.dev/system/string/1.0.0";
		//
		// var bytes = Encoding.UTF8.GetBytes(url);
		// var hexD = bytes.ToHexadecimal();
		// PrintVariable(hexD);
		// var base64 = bytes.ToBase64UrlString();
		// PrintVariable(base64);
		// var urlBack = Encoding.UTF8.GetString(base64.FromBase64UrlString());
		// PrintVariable(urlBack);
		
		// var url =
		// 	$"https://generic_reset.com/1.0.0?__generic1=aHR0cHM6Ly9tZXRhLmZ1eGlvbi5kZXYvc3lzdGVtL2ludC8xLjAuMA&__generic2=aHR0cHM6Ly9nZW5lcmljLmNvbS8xLjAuMD9fX2dlbmVyaWMxPWFIUjBjSE02THk5dFpYUmhMbVoxZUdsdmJpNWtaWFl2YzNsemRHVnRMMmx1ZEM4eExqQXVNQSZfX2dlbmVyaWMyPWFIUjBjSE02THk5dFpYUmhMbVoxZUdsdmJpNWtaWFl2YzNsemRHVnRMM04wY21sdVp5OHhMakF1TUE&__interface=aHR0cHM6Ly9pbnRlcmZhY2UxLmNvbS8xLjAuMA&__interface=aHR0cHM6Ly9pbnRlcmZhY2UyLmNvbS8xLjAuMA&__base1=aHR0cHM6Ly9nZW5lcmljLmNvbS9HZW5lcmljX0VjaGVsb24xLzEuMC4wP19fZ2VuZXJpYzE9YUhSMGNITTZMeTl0WlhSaExtWjFlR2x2Ymk1a1pYWXZjM2x6ZEdWdEwybHVkQzh4TGpBdU1BJl9fZ2VuZXJpYzI9YUhSMGNITTZMeTluWlc1bGNtbGpMbU52YlM4eExqQXVNRDlmWDJkbGJtVnlhV014UFdGSVVqQmpTRTAyVEhrNWRGcFlVbWhNYlZveFpVZHNkbUpwTld0YVdGbDJZek5zZW1SSFZuUk1NbXgxWkVNNGVFeHFRWFZOUVNaZlgyZGxibVZ5YVdNeVBXRklVakJqU0UwMlRIazVkRnBZVW1oTWJWb3haVWRzZG1KcE5XdGFXRmwyWXpOc2VtUkhWblJNTTA0d1kyMXNkVnA1T0hoTWFrRjFUVUU";
		// var uri = new Uri(url);
		// var pars = HttpUtility.ParseQueryString(uri.Query);
		// PrintVariable(pars[UriKey.InterfaceParameterName]);
		// var inters = pars[UriKey.InterfaceParameterName]?.Split(',');
		// Assert.NotNull(inters);
		// foreach(var inter in inters)
		// 	PrintVariable(inter);
	}
	[Fact(Skip = "Debería dar un error en tiempo de compilacion")]
	public void ConstantExpected()
	{
		var value = "";
		UriKey uk = new(value);
	}
	[Fact(DisplayName = "Derived types")]
	public void DerivedTypes()
	{
		void DoType(Type type)
		{
			Output.WriteLine("\t" + type.Name);
			Output.WriteLine("\t\t" + type.GetUriKey());
		}
		void DoTypeThrow<TException>(Type type)
			where TException : Exception
		{
			Output.WriteLine("\t" + type.Name);
			Throws<TException>(() => type.GetUriKey(), "\t\t");
		}
		Output.WriteLine("Chain 1 - Show how alone echelon in chain doesn't works");
		DoTypeThrow<UriKeyInheritanceException>(typeof(Chain1));
		Output.WriteLine("Chain 2 - Show how parameters works");
		DoType(typeof(Chain2));
		DoType(typeof(Chain2_Echelon1));
		Output.WriteLine("Chain 3 - Show how chain reset works");
		DoType(typeof(Chain3));
		DoType(typeof(Chain3_Echelon1));
		DoType(typeof(Chain3Reset1));
		DoType(typeof(Chain3Reset1_Echelon1));
		DoType(typeof(Chain3Reset2));
		DoType(typeof(Chain3Reset2_Echelon1));
		DoTypeThrow<UriKeyResetException>(typeof(Chain3Reset2_Echelon2Reset));
		DoTypeThrow<UriKeyResetException>(typeof(Chain3Reset3WithoutReset));
		DoTypeThrow<UriKeyResetException>(typeof(Chain3Reset3WithoutReset_Echelon1));
		DoTypeThrow<UriKeyResetException>(typeof(Chain3Reset3Based));
		DoTypeThrow<UriKeyResetException>(typeof(Chain3Reset3Based_Echelon1));
		Output.WriteLine("Chain 4 - Show how echelon seal works");
		DoType(typeof(Chain4));
		DoTypeThrow<UriKeySealedException>(typeof(Chain4_SealEchelon1));
		DoTypeThrow<UriKeySealedException>(typeof(Chain4_Echelon2));
		DoTypeThrow<UriKeySealedException>(typeof(Chain4_Echelon3));
		Output.WriteLine("Chain 5 - Show how echelon broken works");
		DoType(typeof(Chain5));
		DoType(typeof(Chain5_Echelon1));
		DoTypeThrow<AttributeNotFoundException>(typeof(Chain5_BrokenEchelon2));
		DoTypeThrow<AttributeNotFoundException>(typeof(Chain5_Echelon3));
		Output.WriteLine("Chain 6 - Show how echelon bypass works");
		DoType(typeof(Chain6));
		DoType(typeof(Chain6_Echelon1));
		DoTypeThrow<UriKeyBypassedException>(typeof(Chain6_BypassEchelon2));
		DoType(typeof(Chain6_Echelon3));
		Output.WriteLine("Chain 7 - Show how echelon reset and seal works together");
		DoType(typeof(Chain7));
		DoType(typeof(Chain7_Echelon1));
		DoTypeThrow<UriKeySealedException>(typeof(Chain7_SealEchelon2));
		DoTypeThrow<UriKeySealedException>(typeof(Chain7_Echelon3));
		DoTypeThrow<UriKeySealedException>(typeof(Chain7_Echelon4));
		DoType(typeof(Chain7Reset));
		DoType(typeof(Chain7Reset_Echelon1));
		Output.WriteLine("Chain 8 - Show how echelon reset and broken works together");
		DoType(typeof(Chain8));
		DoType(typeof(Chain8_Echelon1));
		DoTypeThrow<AttributeNotFoundException>(typeof(Chain8_BrokenEchelon2));
		DoTypeThrow<AttributeNotFoundException>(typeof(Chain8_Echelon3));
		DoType(typeof(Chain8Reset));
		DoType(typeof(Chain8Reset_Echelon1));
		Output.WriteLine("Chain 9 - Show how echelon reset and bypass works together (when bypass is not before rest)");
		DoType(typeof(Chain9));
		DoType(typeof(Chain9_Echelon1));
		DoTypeThrow<UriKeyBypassedException>(typeof(Chain9_BypassEchelon2));
		DoType(typeof(Chain9_Echelon3));
		DoType(typeof(Chain9Reset));
		DoType(typeof(Chain9Reset_Echelon1));
		Output.WriteLine("Chain 10 - Show how echelon reset and bypass works together (when bypass is just before rest)");
		DoType(typeof(Chain10));
		DoType(typeof(Chain10_Echelon1));
		DoTypeThrow<UriKeyBypassedException>(typeof(Chain10_BypassEchelon2));
		DoType(typeof(Chain10Reset));
		DoType(typeof(Chain10Reset_Echelon1));
	}
	[Fact]
	public void DeserializeFromJson()
	{
		PrintVariable("\"https://fuxion.dev/folder/1.0.0\"".DeserializeFromJson<Uri>());
		PrintVariable("\"https://fuxion.dev/folder/1.0.0\"".DeserializeFromJson<UriKey>());
		PrintVariable(
			"\"https://chain9reset.com/Chain9Reset_Folder/Chain9Reset_Echelon1_Folder/1.0.0?__base=https%3A%2F%2Fchain9.com%2FChain9_Folder%2FChain9_Echelon1_Folder%2FChain9_Echelon3_Folder%2F1.0.0\""
				.DeserializeFromJson<UriKey>());
	}
	[Fact]
	public void Dictionary()
	{
		Dictionary<UriKey, string> dic = new();
		UriKey uk1 = new("https://fuxion.dev/folder/1.0.0");
		UriKey uk2 = new("https://fuxion.dev/folder/1.0.0");
		
		dic.Add(uk1, "value");
		IsTrue(dic.ContainsKey(uk1));
		IsTrue(uk1.Equals(uk2));
		IsTrue(dic.ContainsKey(uk2));
	}
	[Fact(DisplayName = "Generic types")]
	public void GenericTypes()
	{
		var t1 = typeof(string).GetUriKey();
		var t2 = typeof(int).GetUriKey();
		var g1 = typeof(Generic<string, string>).GetUriKey();
		var g2 = typeof(Generic<int, int>).GetUriKey();
		var g3 = typeof(Generic<,>).GetUriKey();
		var gd1 = typeof(Generic_Echelon1<string, string>).GetUriKey();
		var gd2 = typeof(Generic_Echelon1<int, int>).GetUriKey();
		var gr1 = typeof(Generic_Reset<string, string>).GetUriKey();
		var gr2 = typeof(Generic_Reset<Class, Generic<int, string>>).GetUriKey();
		
		PrintVariable(t1);
		PrintVariable(t2);
		PrintVariable(g1);
		PrintVariable(g2);
		PrintVariable(g3);
		PrintVariable(gd1);
		PrintVariable(gd2);
		PrintVariable(gr1);
		PrintVariable(gr2);
		AnalyseUriKey(gr2);
	}
	[Fact]
	public void SerializeToJson()
	{
		PrintVariable(new Uri("https://fuxion.dev/folder/1.0.0").SerializeToJson());
		PrintVariable(new UriKey("https://fuxion.dev/folder/1.0.0").SerializeToJson());
		PrintVariable(typeof(Chain9Reset_Echelon1).GetUriKey()
			.SerializeToJson());
	}
	[Fact(DisplayName = "Validate UriKey properties")]
	public void ValidateProperties()
	{
		var i1 = "https://fuxion.dev/interface-1/1.0.0";
		var i1b64 = Encoding.UTF8.GetBytes(i1)
			.ToBase64UrlString();
		var i2 = "https://fuxion.dev/interface-2/1.0.0";
		var i2b64 = Encoding.UTF8.GetBytes(i2)
			.ToBase64UrlString();
		var g1 = "https://fuxion.dev/generic-1/1.0.0"u8.ToArray()
			.ToBase64UrlString();
		var g2 = "https://fuxion.dev/generic-2/1.0.0"u8.ToArray()
			.ToBase64UrlString();
		var c1 = "https://fuxion.dev/chain-1/1.0.0"u8.ToArray()
			.ToBase64UrlString();
		var c2 = "https://fuxion.dev/chain-2/1.0.0"u8.ToArray()
			.ToBase64UrlString();
		// var u1 = new UriKey(new ($"https://fuxion.dev/1.0.0?__interfaces={i1}-{i2}"),true);
		UriBuilder ub1 = new("https://fuxion.dev/1.0.0");
		var query = HttpUtility.ParseQueryString(ub1.Query);
		query[UriKey.InterfacesParameterName] = $"{i1b64}{UriKey.ParameterSeparator}{i2b64}";
		ub1.Query += query;
		var u1 = new UriKey(ub1.Uri, true);
		AnalyseUriKey(u1);
		Assert.Equal(i1, u1.Interfaces.First()
			.ToString());
		Assert.Equal(i2, u1.Interfaces.Last()
			.ToString());
	}
	[Fact(DisplayName = "Validate UriKeyAttribute")]
	public void ValidateUriKeyAttribute()
	{
		// Absolute Uris
		PrintVariable(new UriKeyAttribute("https://fuxion.dev/one/two/1.0.0").Uri.ToString());
		PrintVariable(new UriKeyAttribute("https://@fuxion.dev/one/two/1.0.0").Uri.ToString());
		PrintVariable(new UriKeyAttribute("https://fuxion.dev:443/one/two/1.0.0").Uri.ToString());
		PrintVariable(new UriKeyAttribute("http://fuxion.dev:8000/one/two/1.0.0").Uri.ToString());
		PrintVariable(new UriKeyAttribute("https://fuxion.dev/one/two/1.0.0?parameter=value").Uri.ToString());
		Throws<UriKeySemanticVersionException>(() => new UriKeyAttribute("https://fuxion.dev"));
		Throws<UriKeySemanticVersionException>(() => new UriKeyAttribute("https://fuxion.dev/one/two/1.0.0/"));
		Throws<UriKeyUserInfoException>(() => new UriKeyAttribute("https://userInfo@fuxion.dev/one/two/1.0.0"));
		Throws<UriKeyFragmentException>(() => new UriKeyAttribute("https://fuxion.dev/one/two/1.0.0#"));
		Throws<UriKeyFragmentException>(() => new UriKeyAttribute("https://fuxion.dev/one/two/1.0.0#fragment"));
		Throws<UriKeyParameterException>(() => new UriKeyAttribute($"https://fuxion.dev/one/two/1.0.0?{UriKey.InterfacesParameterName}=fail"));
		Throws<UriKeyParameterException>(() => new UriKeyAttribute($"https://fuxion.dev/one/two/1.0.0?{UriKey.GenericsParameterName}=fail"));
		Throws<UriKeyParameterException>(() => new UriKeyAttribute($"https://fuxion.dev/one/two/1.0.0?{UriKey.BasesParameterName}=fail"));
		
		// Relative Uris
		PrintVariable(new UriKeyAttribute("one/two/1.0.0").Uri.ToString());
		PrintVariable(new UriKeyAttribute("one/two/1.0.0?parameter=value").Uri.ToString());
		Throws<UriKeySemanticVersionException>(() => new UriKeyAttribute("one/two"));
		Throws<UriKeyPathException>(() => new UriKeyAttribute("/one/two/1.0.0"));
		Throws<UriKeyFragmentException>(() => new UriKeyAttribute("one/two/1.0.0#fragment"));
		Throws<UriKeyParameterException>(() => new UriKeyAttribute($"one/two/1.0.0?{UriKey.InterfacesParameterName}=fail"));
	}
}

[UriKey($"https://{nameof(Interface1)}.com/1.0.0")]
public interface Interface1 { }

[UriKey($"https://{nameof(GenericInterface<string>)}.com/1.0.0")]
public interface GenericInterface<T> : Interface1 { }

[UriKey($"https://{nameof(Class)}.com/1.0.0")]
public class Class : Interface1 { }

[UriKey($"https://{nameof(Generic<string, string>)}.com/1.0.0")]
public class Generic<T1, T2> { }

[UriKey($"{nameof(Generic_Echelon1<string, string>)}/1.0.0")]
public class Generic_Echelon1<T1, T2> : Generic<T1, T2> { }

[UriKey($"https://{nameof(Generic_Reset<string, string>)}.com/1.0.0?param1=value1", isReset: true)]
public class Generic_Reset<T1, T2> : Generic_Echelon1<T1, T2>, GenericInterface<byte> { }

#region Chain 1 - Show how alone echelon in chain doesn't works
[UriKey($"{nameof(Chain1)}_Folder/1.0.0")]
class Chain1 { }
#endregion

#region Chain 2 - Show how parameters works
[UriKey($"https://{nameof(Chain2)}.com/{nameof(Chain2)}_Folder/1.0.0?{nameof(Chain2)}_Parameter={nameof(Chain2)}_Value")]
class Chain2 { }

[UriKey($"{nameof(Chain2_Echelon1)}_Folder/1.0.0?{nameof(Chain2_Echelon1)}_Parameter={nameof(Chain2_Echelon1)}_Value")]
class Chain2_Echelon1 : Chain2 { }
#endregion

#region Chain 3 - Show how chain reset works
[UriKey($"https://{nameof(Chain3)}.com/{nameof(Chain3)}_Folder/1.0.0")]
class Chain3 { }

[UriKey($"{nameof(Chain3_Echelon1)}_Folder/1.0.0")]
class Chain3_Echelon1 : Chain3 { }

// This reset the chain because is an absolute uri
[UriKey($"https://{nameof(Chain3Reset1)}.com/{nameof(Chain3Reset1)}_Folder/1.0.0", isReset: true)]
class Chain3Reset1 : Chain3_Echelon1 { }

[UriKey($"{nameof(Chain3Reset1_Echelon1)}_Folder/1.0.0")]
class Chain3Reset1_Echelon1 : Chain3Reset1 { }

// This reset the chain because is an absolute uri
[UriKey($"https://{nameof(Chain3Reset2)}.com/{nameof(Chain3Reset2)}_Folder/1.0.0", isReset: true)]
class Chain3Reset2 : Chain3Reset1_Echelon1 { }

[UriKey($"{nameof(Chain3Reset2_Echelon1)}_Folder/1.0.0")]
class Chain3Reset2_Echelon1 : Chain3Reset2 { }

// This FAILS because you cannot reset the chain with a relative uri
[UriKey($"{nameof(Chain3Reset2_Echelon2Reset)}_Folder/1.0.0", isReset: true)]
class Chain3Reset2_Echelon2Reset : Chain3Reset2_Echelon1 { }

// This FAILS because you must set isReset parameter to reset the chain
[UriKey($"https://{nameof(Chain3Reset3WithoutReset)}.com/{nameof(Chain3Reset3WithoutReset_Echelon1)}_Folder/1.0.0")]
class Chain3Reset3WithoutReset : Chain3Reset2_Echelon1 { }

[UriKey($"{nameof(Chain3Reset2_Echelon1)}_Folder/1.0.0")]
class Chain3Reset3WithoutReset_Echelon1 : Chain3Reset3WithoutReset { }

// This FAILS because the reset uri cannot be based on previous uri
[UriKey($"https://{nameof(Chain3Reset2)}.com/{nameof(Chain3Reset2)}_Folder/moreFolders/1.0.0", isReset: true)]
class Chain3Reset3Based : Chain3Reset2_Echelon1 { }

[UriKey($"{nameof(Chain3Reset3Based_Echelon1)}_Folder/1.0.0")]
class Chain3Reset3Based_Echelon1 : Chain3Reset3Based { }
#endregion

#region Chain 4 - Show how chain seal works
[UriKey($"https://{nameof(Chain4)}.com/{nameof(Chain4)}_Folder/1.0.0")]
class Chain4 { }

[UriKey($"{nameof(Chain4_SealEchelon1)}_Folder/1.0.0", true)]
class Chain4_SealEchelon1 : Chain4 { }

[UriKey($"{nameof(Chain4_Echelon2)}_Folder/1.0.0")]
class Chain4_Echelon2 : Chain4_SealEchelon1 { }

[UriKey($"{nameof(Chain4_Echelon3)}_Folder/1.0.0")]
class Chain4_Echelon3 : Chain4_Echelon2 { }
#endregion

#region Chain 5 - Show how echelon broken works
[UriKey($"https://{nameof(Chain5)}.com/{nameof(Chain5)}_Folder/1.0.0")]
class Chain5 { }

[UriKey($"{nameof(Chain5_Echelon1)}_Folder/1.0.0")]
class Chain5_Echelon1 : Chain5 { }

class Chain5_BrokenEchelon2 : Chain5_Echelon1 { }

[UriKey($"{nameof(Chain5_Echelon3)}_Folder/1.0.0")]
class Chain5_Echelon3 : Chain5_BrokenEchelon2 { }
#endregion

#region Chain 6 - Show how echelon bypass works
[UriKey($"https://{nameof(Chain6)}.com/{nameof(Chain6)}_Folder/1.0.0")]
class Chain6 { }

[UriKey($"{nameof(Chain6_Echelon1)}_Folder/1.0.0")]
class Chain6_Echelon1 : Chain6 { }

[UriKeyBypass]
class Chain6_BypassEchelon2 : Chain6_Echelon1 { }

[UriKey($"{nameof(Chain6_Echelon3)}_Folder/1.0.0")]
class Chain6_Echelon3 : Chain6_BypassEchelon2 { }
#endregion

#region Chain 7 - Show how echelon reset and seal works together
[UriKey($"https://{nameof(Chain7)}.com/{nameof(Chain7)}_Folder/1.0.0")]
class Chain7 { }

[UriKey($"{nameof(Chain7_Echelon1)}_Folder/1.0.0")]
class Chain7_Echelon1 : Chain7 { }

[UriKey($"{nameof(Chain7_SealEchelon2)}_Folder/1.0.0", true)]
class Chain7_SealEchelon2 : Chain7_Echelon1 { }

[UriKey($"{nameof(Chain7_Echelon3)}_Folder/1.0.0")]
class Chain7_Echelon3 : Chain7_SealEchelon2 { }

[UriKey($"{nameof(Chain7_Echelon4)}_Folder/1.0.0")]
class Chain7_Echelon4 : Chain7_Echelon3 { }

[UriKey($"https://{nameof(Chain7Reset)}.com/{nameof(Chain7Reset)}_Folder/1.0.0", isReset: true)]
class Chain7Reset : Chain7_Echelon3 { }

[UriKey($"{nameof(Chain7Reset_Echelon1)}_Folder/1.0.0")]
class Chain7Reset_Echelon1 : Chain7Reset { }
#endregion

#region Chain 8 - Show how echelon reset and broken works together
[UriKey($"https://{nameof(Chain8)}.com/{nameof(Chain8)}_Folder/1.0.0")]
class Chain8 { }

[UriKey($"{nameof(Chain8_Echelon1)}_Folder/1.0.0")]
class Chain8_Echelon1 : Chain8 { }

class Chain8_BrokenEchelon2 : Chain8_Echelon1 { }

[UriKey($"{nameof(Chain8_Echelon3)}_Folder/1.0.0")]
class Chain8_Echelon3 : Chain8_BrokenEchelon2 { }

[UriKey($"https://{nameof(Chain8Reset)}.com/{nameof(Chain8Reset)}_Folder/1.0.0", isReset: true)]
class Chain8Reset : Chain8_Echelon3 { }

[UriKey($"{nameof(Chain8Reset_Echelon1)}_Folder/1.0.0")]
class Chain8Reset_Echelon1 : Chain8Reset { }
#endregion

#region Chain 9 - Show how echelon reset and bypass works together (when bypass is not before rest)
[UriKey($"https://{nameof(Chain9)}.com/{nameof(Chain9)}_Folder/1.0.0")]
class Chain9 { }

[UriKey($"{nameof(Chain9_Echelon1)}_Folder/1.0.0")]
class Chain9_Echelon1 : Chain9 { }

[UriKeyBypass]
class Chain9_BypassEchelon2 : Chain9_Echelon1 { }

[UriKey($"{nameof(Chain9_Echelon3)}_Folder/1.0.0")]
class Chain9_Echelon3 : Chain9_BypassEchelon2 { }

[UriKey($"https://{nameof(Chain9Reset)}.com/{nameof(Chain9Reset)}_Folder/1.0.0", isReset: true)]
class Chain9Reset : Chain9_Echelon3 { }

[UriKey($"{nameof(Chain9Reset_Echelon1)}_Folder/1.0.0")]
class Chain9Reset_Echelon1 : Chain9Reset { }
#endregion

#region Chain 10 - Show how echelon reset and bypass works together (when bypass is just before rest)
[UriKey($"https://{nameof(Chain10)}.com/{nameof(Chain10)}_Folder/1.0.0")]
class Chain10 { }

[UriKey($"{nameof(Chain10_Echelon1)}_Folder/1.0.0")]
class Chain10_Echelon1 : Chain10 { }

[UriKeyBypass]
class Chain10_BypassEchelon2 : Chain10_Echelon1 { }

[UriKey($"https://{nameof(Chain10Reset)}.com/{nameof(Chain10Reset)}_Folder/1.0.0", isReset: true)]
class Chain10Reset : Chain10_BypassEchelon2 { }

[UriKey($"{nameof(Chain10Reset_Echelon1)}_Folder/1.0.0")]
class Chain10Reset_Echelon1 : Chain10Reset { }
#endregion