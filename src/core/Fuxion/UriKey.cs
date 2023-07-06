using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Web;

namespace Fuxion;

[JsonConverter(typeof(UriKeyJsonConverter))]
public class UriKey: IEquatable<UriKey>//, IComparable, IComparable<UriKey>
{
	public const string InterfaceParameterName = "__interface";
	public const string BaseParameterNamePrefix = "__base";
	public const string FuxionBaseUri = "https://meta.fuxion.dev/";
	public const string FuxionSystemTypesBaseUri = FuxionBaseUri+"system/";

	public UriKey(string key)
	{
		(Uri, Bases, Version) = ValidateAndNormalizeUri(new(key), false);
	}
	public UriKey(Uri uri)
	{
		(Uri, Bases, Version) = ValidateAndNormalizeUri(uri, false);
	}
	internal UriKey(Uri uri, bool allowReservedParameters)
	{
		(Uri, Bases, Version) = ValidateAndNormalizeUri(uri, allowReservedParameters);
	}
	
	public Uri Uri { get; }
	public Uri[] Bases { get; }
	public SemanticVersion Version { get; }
	internal static (Uri Uri, Uri[] Bases, SemanticVersion Version) ValidateAndNormalizeUri(Uri uri, bool allowReservedParameters)
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
			if (pars.AllKeys.Any(key => key == InterfaceParameterName)) throw new UriKeyParameterException($"Neither parameter in query can be named '{InterfaceParameterName}'");
			if (pars.AllKeys.Any(key => key?.StartsWith(BaseParameterNamePrefix) ?? false)) throw new UriKeyParameterException($"Neither parameter in query can be named started with '{BaseParameterNamePrefix}'");
		}

		// If the source uri is relative, return it
		if (!uri.IsAbsoluteUri) return (uri, Array.Empty<Uri>(), version);
		
		// Use UriBuilder to normalize the uri
		UriBuilder ub = new(currentUri);
		ub.UserName = null;
		if (ub.Uri.IsDefaultPort) ub.Port = -1;
		currentUri = ub.Uri;
		
		// Extract bases
		List<Uri> bases = new();
		for (var i = 1;; i++)
		{
			var par = pars[BaseParameterNamePrefix + i];
			if(par is not null)
			//if (pars.AllKeys.Contains(BaseParameterNamePrefix + i))
			{
				bases.Add(new(System.Uri.UnescapeDataString(par)));
			} else break;
		}
		
		return (currentUri, bases.ToArray(), version);
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
	// 		
	// 		
	// 		
	// 		
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