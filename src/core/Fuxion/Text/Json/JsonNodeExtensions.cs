using System.Text.Json.Nodes;

namespace Fuxion.Text.Json;

public static class JsonNodeExtensions
{
	public static string ToJsonString(this JsonNode me, bool writeIndented)
		=> me.ToJsonString(new()
		{
			WriteIndented = writeIndented
		});
}