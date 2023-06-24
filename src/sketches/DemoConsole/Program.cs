using System.Text.Json.Serialization;
using Fuxion.Licensing;
using Xunit;

namespace DemoConsole;

public class Program
{
	public static void Main(string[] args)
	{
		var hardwareId = Guid.NewGuid().ToString();
		var productId = Guid.NewGuid().ToString();
		var lic = new LicenseMock();
		lic.SetHarwareId(hardwareId);
		lic.SetProductId(productId);
		var con = new LicenseContainer("signature", lic);
		Console.WriteLine("ToJson:");
		var json = con.SerializeToJson();
		Console.WriteLine(json);
		Console.WriteLine("FromJson:");
		var con2 = json.DeserializeFromJson<LicenseContainer>()!;
		var json2 = con2.SerializeToJson();
		Console.WriteLine(json2);
		Assert.Equal(json, json2);
		Assert.True(con2.Is<LicenseMock>());
		var lic2 = con2.As<LicenseMock>()!;
		Assert.NotNull(lic2);
		Assert.Equal(lic.HardwareId.Key, lic2.HardwareId.Key);
		Assert.Equal(lic.ProductId.Key, lic2.ProductId.Key);

		//Assert.Equal(con2.LicenseAs<LicenseMock>(), lic);
		var time = new Random((int)DateTime.Now.Ticks).Next(500, 1500);
		Console.WriteLine("Time: " + time);
		Thread.Sleep(time);
		Console.WriteLine("FromJson timed:");
		var con3 = json.DeserializeFromJson<LicenseContainer>()!;
		var json3 = con3.SerializeToJson();
		Console.WriteLine(json3);
		Assert.Equal(json, json3);
		Assert.Equal(json2, json3);
		Assert.True(con3.Is<LicenseMock>());
		var lic3 = con3.As<LicenseMock>()!;
		Assert.NotNull(lic3);
		Assert.Equal(lic.HardwareId.Key, lic3.HardwareId.Key);
		Assert.Equal(lic.ProductId.Key, lic3.ProductId.Key);

		//Assert.Equal(con3.LicenseAs<LicenseMock>(), lic);
	}
}

public class LicenseMock : License
{
	public LicenseMock()
	{
		ExpirationUtcTime = new(DateTime.UtcNow.AddYears(1));
		DeactivationUtcTime = new(DateTime.UtcNow.AddMonths(1));
		HardwareId = new(null);
		ProductId = new(null);
	}
	[JsonConstructor]
	public LicenseMock(KeyModelLicenseConstraint hardwareId, KeyModelLicenseConstraint productId, TimeLimitLicenseConstraint deactivationUtcTime, TimeLimitLicenseConstraint expirationUtcTime) : this()
	{
		HardwareId = hardwareId;
		ProductId = productId;
		ExpirationUtcTime = expirationUtcTime;
		DeactivationUtcTime = deactivationUtcTime;
	}
	public TimeLimitLicenseConstraint DeactivationUtcTime { get; }
	public TimeLimitLicenseConstraint ExpirationUtcTime { get; }
	[JsonInclude]
	public KeyModelLicenseConstraint HardwareId { get; private set; }
	[JsonInclude]
	public KeyModelLicenseConstraint ProductId { get; private set; }
	protected override bool Validate(out string validationMessage)
	{
		var res = base.Validate(out validationMessage);
		res = res && ProductId.Validate(out validationMessage, Const.PRODUCT_ID);
		res = res && HardwareId.Validate(out validationMessage, Const.HARDWARE_ID);
		res = res && DeactivationUtcTime.Validate(out validationMessage);
		res = res && ExpirationUtcTime.Validate(out validationMessage);
		return res;
	}
	internal void SetHarwareId(string hardwareId) => HardwareId = new(hardwareId);
	internal void SetProductId(string productId) => ProductId = new(productId);
}

public static class Const
{
	public const string FULL_KEY = "<RSAKeyValue>" +
		"<Modulus>xXnL2ppHKz084JHYmmX2TXSlxMnbq6FDRpRe7aLhShr/HD1KgKyhK54N/uigy6fJR8bPUr0mso8XdSgDI/A8Q/62DwPgNq9wc+SuGylO5laoN9MU9fycK0ntGk7rwW7raL+jWe3W68xQLZKptF3mzBjHW15hM+6b9+vllv2dcok=</Modulus>"
		+
		"<Exponent>AQAB</Exponent>" +
		"<P>4DwUd5X3rmvqnK0SkG0asYsjgNFj0bYaXu6heJnd9zknuPGVl/fP2jOtoyghT/CdXCgfd0gpjMpsEvBFYu3a/w==</P>" +
		"<Q>4XNMWZVQhE84lAWA/t8qCUs8ZtgxlQsqXyb3Z9XxWeT8NMAclVpJQRaOX/qo8GImhSQTdBN8Mt1o3WB0yPFadw==</Q>" +
		"<DP>wUpo5jDLAXqbEZWLVB4IjZT/9LIKlqKgFscjP597G/oTyLPPHOGMNW6oteUI2izyqJcZkKwOzQNMqAKf/UhFpw==</DP>" +
		"<DQ>aydu2YFDdK9ml8wJ5JnTE/nDaqpE3q8g43rUynCANxbD3JqWu1HfUWVUJEAx/ZbY8h0Ude4w8MgVaGrI9xznvw==</DQ>" +
		"<InverseQ>Nhubp4i7E11cf8LsuMRm9jyoHtRloScQP2r9uvmilrRlb2MFKN9/DQbzJa1yIRVETS8M/F/DwnkB6fnBG/9rmg==</InverseQ>" +
		"<D>ZQhNbchlBRBNpy+3PDdSbopxjV8hTowxGVkrwDUHQpzRTKdnCLJJu0EgM/zc15U+e8SRqekwdUaUX9Ja2PY+Pj1uv40d2K5hLasH03bORdSs5eWCiGXjGvUNCyF1tIfez4kCNCthSVJckhw2kG0VoaaREj7W5snLCmLg5KFCzYk=</D>" +
		"</RSAKeyValue>";
	public const string PUBLIC_KEY = "<RSAKeyValue>" +
		"<Modulus>xXnL2ppHKz084JHYmmX2TXSlxMnbq6FDRpRe7aLhShr/HD1KgKyhK54N/uigy6fJR8bPUr0mso8XdSgDI/A8Q/62DwPgNq9wc+SuGylO5laoN9MU9fycK0ntGk7rwW7raL+jWe3W68xQLZKptF3mzBjHW15hM+6b9+vllv2dcok=</Modulus>"
		+
		"<Exponent>AQAB</Exponent>" +
		"</RSAKeyValue>";
	public const string HARDWARE_ID = nameof(HARDWARE_ID);
	public const string PRODUCT_ID = nameof(PRODUCT_ID);
}