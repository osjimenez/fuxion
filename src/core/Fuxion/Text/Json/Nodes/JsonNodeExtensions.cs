namespace System.Text.Json.Nodes;

public static class JsonNodeExtensions
{
	public static string ToJsonString(this JsonNode me, bool writeIndented)
		=> me.ToJsonString(new()
		{
			WriteIndented = writeIndented
		});
}