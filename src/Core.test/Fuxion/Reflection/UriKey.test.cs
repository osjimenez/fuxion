using Fuxion.Reflection;

namespace Fuxion.Test.Reflection;

public class UriKeyTest(ITestOutputHelper output) : BaseTest<TypeKeyTest>(output)
{
	[Fact]
	public void SerializeToJson()
	{
		PrintVariable(new Uri("https://fuxion.dev/folder/1.0.0").SerializeToJson());
		PrintVariable(new UriKey("https://fuxion.dev/folder/1.0.0").SerializeToJson());
		PrintVariable(typeof(Chain9Reset_Echelon1).GetUriKey().SerializeToJson());
	}
	[Fact]
	public void DeserializeFromJson()
	{
		PrintVariable("\"https://fuxion.dev/folder/1.0.0\"".DeserializeFromJson<Uri>());
		PrintVariable("\"https://fuxion.dev/folder/1.0.0\"".DeserializeFromJson<UriKey>());
		PrintVariable("\"https://chain9reset.com/Chain9Reset_Folder/Chain9Reset_Echelon1_Folder/1.0.0?__base=https%3A%2F%2Fchain9.com%2FChain9_Folder%2FChain9_Echelon1_Folder%2FChain9_Echelon3_Folder%2F1.0.0\"".DeserializeFromJson<UriKey>());
	}
	[Fact]
	public void Dictionary()
	{
		Dictionary<UriKey, string> dic = new();
		UriKey uk1 = new("https://fuxion.dev/folder/1.0.0");
		UriKey uk2 = new("https://fuxion.dev/folder/1.0.0");
		
		dic.Add(uk1,"value");
		IsTrue(dic.ContainsKey(uk1));
		IsTrue(uk1.Equals(uk2));
		IsTrue(dic.ContainsKey(uk2));
	}
	[Fact(DisplayName = "Derived types")]
	public void DerivedTypes()
	{
		void DoType(Type type)
		{
			Output.WriteLine("\t" + type.Name);
			try
			{
				var tk = type.GetUriKey();
				Output.WriteLine("\t\t" + tk);
			} catch (Exception ex)
			{
				Output.WriteLine($"\t\t{ex.GetType().Name}: {ex.Message}");
			}
		}
		Output.WriteLine("Chain 1 - Show how alone echelon in chain doesn't works");
		DoType(typeof(Chain1));
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
		Output.WriteLine("Chain 4 - Show how echelon seal works");
		DoType(typeof(Chain4));
		DoType(typeof(Chain4_SealEchelon1));
		DoType(typeof(Chain4_Echelon2));
		DoType(typeof(Chain4_Echelon3));
		Output.WriteLine("Chain 5 - Show how echelon broken works");
		DoType(typeof(Chain5));
		DoType(typeof(Chain5_Echelon1));
		DoType(typeof(Chain5_BrokenEchelon2));
		DoType(typeof(Chain5_Echelon3));
		Output.WriteLine("Chain 6 - Show how echelon bypass works");
		DoType(typeof(Chain6));
		DoType(typeof(Chain6_Echelon1));
		DoType(typeof(Chain6_BypassEchelon2));
		DoType(typeof(Chain6_Echelon3));
		Output.WriteLine("Chain 7 - Show how echelon reset and seal works together");
		DoType(typeof(Chain7));
		DoType(typeof(Chain7_Echelon1));
		DoType(typeof(Chain7_SealEchelon2));
		DoType(typeof(Chain7_Echelon3));
		DoType(typeof(Chain7_Echelon4));
		DoType(typeof(Chain7Reset));
		DoType(typeof(Chain7Reset_Echelon1));
		Output.WriteLine("Chain 8 - Show how echelon reset and broken works together");
		DoType(typeof(Chain8));
		DoType(typeof(Chain8_Echelon1));
		DoType(typeof(Chain8_BrokenEchelon2));
		DoType(typeof(Chain8_Echelon3));
		DoType(typeof(Chain8Reset));
		DoType(typeof(Chain8Reset_Echelon1));
		Output.WriteLine("Chain 9 - Show how echelon reset and bypass works together (when bypass is not before rest)");
		DoType(typeof(Chain9));
		DoType(typeof(Chain9_Echelon1));
		DoType(typeof(Chain9_BypassEchelon2));
		DoType(typeof(Chain9_Echelon3));
		DoType(typeof(Chain9Reset));
		DoType(typeof(Chain9Reset_Echelon1));
		Output.WriteLine("Chain 10 - Show how echelon reset and bypass works together (when bypass is just before rest)");
		DoType(typeof(Chain10));
		DoType(typeof(Chain10_Echelon1));
		DoType(typeof(Chain10_BypassEchelon2));
		DoType(typeof(Chain10Reset));
		DoType(typeof(Chain10Reset_Echelon1));
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
		Throws<UriKeyParameterException>(() => new UriKeyAttribute("https://fuxion.dev/one/two/1.0.0?__interface=fail"));

		// Relative Uris
		PrintVariable(new UriKeyAttribute("one/two/1.0.0").Uri.ToString());
		PrintVariable(new UriKeyAttribute("one/two/1.0.0?parameter=value").Uri.ToString());
		Throws<UriKeySemanticVersionException>(() => new UriKeyAttribute("one/two"));
		Throws<UriKeyPathException>(() => new UriKeyAttribute("/one/two/1.0.0"));
		Throws<UriKeyFragmentException>(() => new UriKeyAttribute("one/two/1.0.0#fragment"));
		Throws<UriKeyParameterException>(() => new UriKeyAttribute("one/two/1.0.0?__interface=fail"));
	}
}

#region Chain 1 - Show how alone echelon in chain doesn't works
[UriKey($"{nameof(Chain1)}_Folder/1.0.0", false)]
class Chain1 { }
#endregion

#region Chain 2 - Show how parameters works
[UriKey($"https://{nameof(Chain2)}.com/{nameof(Chain2)}_Folder/1.0.0?{nameof(Chain2)}_Parameter={nameof(Chain2)}_Value", false)]
class Chain2 { }

[UriKey($"{nameof(Chain2_Echelon1)}_Folder/1.0.0?{nameof(Chain2_Echelon1)}_Parameter={nameof(Chain2_Echelon1)}_Value", false)]
class Chain2_Echelon1 : Chain2 { }
#endregion

#region Chain 3 - Show how chain reset works
[UriKey($"https://{nameof(Chain3)}.com/{nameof(Chain3)}_Folder/1.0.0", false)]
class Chain3 { }

[UriKey($"{nameof(Chain3_Echelon1)}_Folder/1.0.0")]
class Chain3_Echelon1 : Chain3 { }

// This reset the chain because is an absolute uri
[UriKey($"https://{nameof(Chain3Reset1)}.com/{nameof(Chain3Reset1)}_Folder/1.0.0", false)]
class Chain3Reset1 : Chain3_Echelon1 { }

[UriKey($"{nameof(Chain3Reset1_Echelon1)}_Folder/1.0.0")]
class Chain3Reset1_Echelon1 : Chain3Reset1 { }

// This reset the chain because is an absolute uri
[UriKey($"https://{nameof(Chain3Reset2)}.com/{nameof(Chain3Reset2)}_Folder/1.0.0", false)]
class Chain3Reset2 : Chain3Reset1_Echelon1 { }

[UriKey($"{nameof(Chain3Reset2_Echelon1)}_Folder/1.0.0")]
class Chain3Reset2_Echelon1 : Chain3Reset2 { }
#endregion

#region Chain 4 - Show how chain seal works
[UriKey($"https://{nameof(Chain4)}.com/{nameof(Chain4)}_Folder/1.0.0", false)]
class Chain4 { }

[UriKey($"{nameof(Chain4_SealEchelon1)}_Folder/1.0.0")]
class Chain4_SealEchelon1 : Chain4 { }

[UriKey($"{nameof(Chain4_Echelon2)}_Folder/1.0.0")]
class Chain4_Echelon2 : Chain4_SealEchelon1 { }

[UriKey($"{nameof(Chain4_Echelon3)}_Folder/1.0.0")]
class Chain4_Echelon3 : Chain4_Echelon2 { }
#endregion

#region Chain 5 - Show how echelon broken works
[UriKey($"https://{nameof(Chain5)}.com/{nameof(Chain5)}_Folder/1.0.0", false)]
class Chain5 { }

[UriKey($"{nameof(Chain5_Echelon1)}_Folder/1.0.0", false)]
class Chain5_Echelon1 : Chain5 { }

class Chain5_BrokenEchelon2 : Chain5_Echelon1 { }

[UriKey($"{nameof(Chain5_Echelon3)}_Folder/1.0.0")]
class Chain5_Echelon3 : Chain5_BrokenEchelon2 { }
#endregion

#region Chain 6 - Show how echelon bypass works
[UriKey($"https://{nameof(Chain6)}.com/{nameof(Chain6)}_Folder/1.0.0", false)]
class Chain6 { }

[UriKey($"{nameof(Chain6_Echelon1)}_Folder/1.0.0", false)]
class Chain6_Echelon1 : Chain6 { }

[UriKeyBypass]
class Chain6_BypassEchelon2 : Chain6_Echelon1 { }

[UriKey($"{nameof(Chain6_Echelon3)}_Folder/1.0.0")]
class Chain6_Echelon3 : Chain6_BypassEchelon2 { }
#endregion

#region Chain 7 - Show how echelon reset and seal works together
[UriKey($"https://{nameof(Chain7)}.com/{nameof(Chain7)}_Folder/1.0.0", false)]
class Chain7 { }

[UriKey($"{nameof(Chain7_Echelon1)}_Folder/1.0.0", false)]
class Chain7_Echelon1 : Chain7 { }

[UriKey($"{nameof(Chain7_SealEchelon2)}_Folder/1.0.0")]
class Chain7_SealEchelon2 : Chain7_Echelon1 { }

[UriKey($"{nameof(Chain7_Echelon3)}_Folder/1.0.0", false)]
class Chain7_Echelon3 : Chain7_SealEchelon2 { }

[UriKey($"{nameof(Chain7_Echelon4)}_Folder/1.0.0")]
class Chain7_Echelon4 : Chain7_Echelon3 { }

[UriKey($"https://{nameof(Chain7Reset)}.com/{nameof(Chain7Reset)}_Folder/1.0.0", false)]
class Chain7Reset : Chain7_Echelon3 { }

[UriKey($"{nameof(Chain7Reset_Echelon1)}_Folder/1.0.0", false)]
class Chain7Reset_Echelon1 : Chain7Reset { }
#endregion

#region Chain 8 - Show how echelon reset and broken works together
[UriKey($"https://{nameof(Chain8)}.com/{nameof(Chain8)}_Folder/1.0.0", false)]
class Chain8 { }

[UriKey($"{nameof(Chain8_Echelon1)}_Folder/1.0.0", false)]
class Chain8_Echelon1 : Chain8 { }

class Chain8_BrokenEchelon2 : Chain8_Echelon1 { }

[UriKey($"{nameof(Chain8_Echelon3)}_Folder/1.0.0")]
class Chain8_Echelon3 : Chain8_BrokenEchelon2 { }

[UriKey($"https://{nameof(Chain8Reset)}.com/{nameof(Chain8Reset)}_Folder/1.0.0", false)]
class Chain8Reset : Chain8_Echelon3 { }

[UriKey($"{nameof(Chain8Reset_Echelon1)}_Folder/1.0.0", false)]
class Chain8Reset_Echelon1 : Chain8Reset { }
#endregion

#region Chain 9 - Show how echelon reset and bypass works together (when bypass is not before rest)
[UriKey($"https://{nameof(Chain9)}.com/{nameof(Chain9)}_Folder/1.0.0", false)]
class Chain9 { }

[UriKey($"{nameof(Chain9_Echelon1)}_Folder/1.0.0", false)]
class Chain9_Echelon1 : Chain9 { }

[UriKeyBypass]
class Chain9_BypassEchelon2 : Chain9_Echelon1 { }

[UriKey($"{nameof(Chain9_Echelon3)}_Folder/1.0.0")]
class Chain9_Echelon3 : Chain9_BypassEchelon2 { }

[UriKey($"https://{nameof(Chain9Reset)}.com/{nameof(Chain9Reset)}_Folder/1.0.0", false)]
class Chain9Reset : Chain9_Echelon3 { }

[UriKey($"{nameof(Chain9Reset_Echelon1)}_Folder/1.0.0", false)]
class Chain9Reset_Echelon1 : Chain9Reset { }
#endregion

#region Chain 10 - Show how echelon reset and bypass works together (when bypass is just before rest)
[UriKey($"https://{nameof(Chain10)}.com/{nameof(Chain10)}_Folder/1.0.0", false)]
class Chain10 { }

[UriKey($"{nameof(Chain10_Echelon1)}_Folder/1.0.0", false)]
class Chain10_Echelon1 : Chain9 { }

[UriKeyBypass]
class Chain10_BypassEchelon2 : Chain10_Echelon1 { }

[UriKey($"https://{nameof(Chain10Reset)}.com/{nameof(Chain10Reset)}_Folder/1.0.0", false)]
class Chain10Reset : Chain10_BypassEchelon2 { }

[UriKey($"{nameof(Chain10Reset_Echelon1)}_Folder/1.0.0", false)]
class Chain10Reset_Echelon1 : Chain10Reset { }
#endregion