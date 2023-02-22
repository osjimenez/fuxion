namespace Fuxion.Test.Text.Json.Serialization;

public class FallbackConverterTest : BaseTest<FallbackConverterTest>
{
	public FallbackConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "FallbackConverter - Serialize")]
	public void FallbackConverter_Serialize()
	{
		try
		{
			try
			{
				Task.Run(() => {
					InvalidProgramException ipex = new("InvalidProgramException message");
					throw ipex;
				}).Wait();
			} catch (Exception ex)
			{
				InvalidOperationException ioex = new("InvalidOperationException message", ex);
				throw ioex;
			}
		} catch (Exception ex)
		{
			var res = ex.ToJson();
			Output.WriteLine("Exception serialized JSON:");
			Output.WriteLine(res);
		}
	}
}