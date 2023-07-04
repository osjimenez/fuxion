using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Web;

namespace Fuxion.Reflection;

[JsonConverter(typeof(UriKeyJsonConverter))]
public class UriKey: IEquatable<UriKey>//, IComparable, IComparable<UriKey>
{
	public const string INTERFACE_PARAMETER_NAME = "__interface";
	public const string BASE_PARAMETER_NAME = "__base";

	public UriKey(string key)
	{
		(Uri, Version) = ValidateAndNormalizeUri(new(key), false);
	}
	public UriKey(Uri uri)
	{
		(Uri, Version) = ValidateAndNormalizeUri(uri, false);
	}
	internal UriKey(Uri uri, bool allowReservedParameters)
	{
		(Uri, Version) = ValidateAndNormalizeUri(uri, allowReservedParameters);
	}
	
	public Uri Uri { get; }
	public SemanticVersion Version { get; }
	internal static (Uri Uri, SemanticVersion Version) ValidateAndNormalizeUri(Uri uri, bool allowReservedParameters)
	{
		// If uri is relative, make it absolute only to process it
		var currentUri = uri;
		if (!uri.IsAbsoluteUri && Uri.TryCreate(new("https://domain.com"), uri, out var tempUri)) currentUri = tempUri;
		
		// Validate that the last segment is a valid semantic version pattern
		var lastSegment = currentUri.Segments.LastOrDefault()
			?? throw new UriKeySemanticVersionException($"Uri must be a last segment with a semantic version valid pattern");
		if (!SemanticVersion.TryParse(lastSegment, out SemanticVersion? version)) throw new UriKeySemanticVersionException($"The last segment '{lastSegment}' isn't a valid semantic version pattern");
		
		// If the uri is relative, checks that not start with '/'
		if(!uri.IsAbsoluteUri && uri.ToString().StartsWith('/'))
			throw new UriKeyPathException($"Relative Uri path cannot begins with '/'");
		
		// Validate that hasn't user info
		// Issue when call IsBaseOf and UserInfo differs
		// https://github.com/dotnet/runtime/issues/88265
		// This a BUG that Microsoft must be resolve
		if (!string.IsNullOrWhiteSpace(currentUri.UserInfo)) throw new UriKeyUserInfoException($"Uri cannot has user info");
		
		// Validate that hasn't fragment
		if (!string.IsNullOrWhiteSpace(currentUri.Fragment)) throw new UriKeyFragmentException($"Uri cannot has fragment");
		
		// Validate that hasn't parameters named with reserved names, if not allowed
		var pars = HttpUtility.ParseQueryString(currentUri.Query);
		if (!allowReservedParameters)
		{
			if (pars.AllKeys.Any(key => key == INTERFACE_PARAMETER_NAME)) throw new UriKeyParameterException($"Neither parameter in query can be named '{INTERFACE_PARAMETER_NAME}'");
			if (pars.AllKeys.Any(key => key == BASE_PARAMETER_NAME)) throw new UriKeyParameterException($"Neither parameter in query can be named '{BASE_PARAMETER_NAME}'");
		}

		// If the source uri is relative, return it
		if (!uri.IsAbsoluteUri) return (uri, version);
		
		// Use UriBuilder to normalize the uri
		UriBuilder ub = new(currentUri);
		ub.UserName = null;
		if (ub.Uri.IsDefaultPort) ub.Port = -1;
		currentUri = ub.Uri;
		
		return (currentUri, version);
	}
	public override string ToString() => Uri.ToString();
	
	// public static bool operator ==(UriKey? identifier1, UriKey? identifier2)
	// {
	// 	if (identifier1 is null) return identifier2 is null;
	// 	return identifier1.Equals(identifier2);
	// }
	// public static bool operator !=(UriKey identifier1, UriKey identifier2) => !(identifier1 == identifier2);
	// public static bool operator < (UriKey identifier1, UriKey identifier2)
	// {
	// 	ArgumentNullException.ThrowIfNull(identifier1);
	// 	return identifier1.CompareTo(identifier2) < 0;
	// }
	// public static bool operator <=(UriKey identifier1, UriKey identifier2) => identifier1 == identifier2 || identifier1 < identifier2;
	// public static bool operator > (UriKey identifier1, UriKey identifier2)
	// {
	// 	ArgumentNullException.ThrowIfNull(identifier1);
	// 	return identifier2 < identifier1;
	// }
	// public static bool operator >=(UriKey identifier1, UriKey identifier2) => identifier1 == identifier2 || identifier1 > identifier2;
	// public int CompareTo(UriKey? other)
	// {
	// 	if (ReferenceEquals(this, other)) return 0;
	// 	if (other is null) return 1;
	// 	var uriRes = Uri.IsBaseOf(other.Uri) ? 
	// 	return (IsNumber, other.IsNumber) switch
	// 	{
	// 		(true, true) => Nullable.Compare(NumberValue, other.NumberValue),
	// 		(true, false) => -1,
	// 		(false, true) => 1,
	// 		var _ => string.Compare(Value, other.Value, StringComparison.Ordinal)
	// 	};
	// }
	// public int CompareTo(object? obj)
	// {
	// 	if (obj is null)
	// 		return 1;
	// 	var other = obj as UriKey
	// 		?? throw new ArgumentException($"Type must be '{nameof(UriKey)}'", "obj");
	// 	return CompareTo(other);
	// }
	public bool Equals(UriKey? other) => other is not null && Uri.Equals(Uri, other.Uri);
	public override bool Equals(object? obj)
	{
		var identifier = obj as UriKey;
		return identifier is not null && Equals(identifier);
	}
	public override int GetHashCode() => Uri.GetHashCode();
}