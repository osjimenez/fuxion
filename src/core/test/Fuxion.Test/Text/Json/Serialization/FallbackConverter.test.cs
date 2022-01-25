namespace Fuxion.Test.Json;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;

public class ExceptionConverterTest : BaseTest
{
	public ExceptionConverterTest(ITestOutputHelper output) : base(output)
	{
		//Printer.WriteLineAction = m => output.WriteLine(m);
	}
	[Fact(DisplayName = "ExceptionConverter - Serialize")]
	public void ExceptionConverter_Serialize()
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
	}
}
