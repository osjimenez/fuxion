namespace Fuxion.Test.Json;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;

public class FallbackConverterTest : BaseTest
{
	public FallbackConverterTest(ITestOutputHelper output) : base(output)
	{
		int t = 8;
		//Printer.WriteLineAction = m => output.WriteLine(m);
	}
	[Fact(DisplayName = "FallbackConverter2 - Serialize")]
	public void FallbackConverter2_Serialize()
	{
		int t = 8;
		Exception ex = new Exception("VAMOSSSSS");
		//JsonSerializerOptions opt = new();
		//opt.WriteIndented = true;
		//opt.Converters.Add(new FallbackConverter<Exception>(new MultilineStringToCollectionPropertyFallbackResolver()));
		//var res = JsonSerializer.Serialize(ex, opt);
		//Output.WriteLine("Exception serialized JSON:");
		//Output.WriteLine(res);
	}
	[Fact(DisplayName = "FallbackConverter - Serialize")]
	public void FallbackConverter_Serialize()
	{
		try
		{
			try
			{
				Task.Run(() =>
				{
					InvalidProgramException ipex = new("InvalidProgramException message");
					throw ipex;
				}).Wait();
			}
			catch (Exception ex)
			{
				InvalidOperationException ioex = new("InvalidOperationException message", ex);
				throw ioex;
			}
		}
		catch (Exception ex)
		{
			JsonSerializerOptions opt = new();
			opt.WriteIndented = true;
			opt.Converters.Add(new FallbackConverter<Exception>(new MultilineStringToCollectionPropertyFallbackResolver()));
			var res = JsonSerializer.Serialize(ex, opt);
			Output.WriteLine("Exception serialized JSON:");
			Output.WriteLine(res);
		}
		//Exception ex = new Exception("VAMOSSSSS");
		//JsonSerializerOptions opt = new();
		//opt.WriteIndented = true;
		//opt.Converters.Add(new FallbackConverter<Exception>(new MultilineStringToCollectionPropertyFallbackResolver()));
		//var res = JsonSerializer.Serialize(ex, opt);
		//Output.WriteLine("Exception serialized JSON:");
		//Output.WriteLine(res);
	}
}
