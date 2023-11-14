using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;

namespace Fuxion;
/*
 * Attribute (Key, Version)
 * Json Pod (Key, Version, Generics)
 * Directory (Full)
 * Layout (Key, Version, Generics, Parameters)
 *
 * 
 * Discriminated (Key, Version, Generics)
 * Full
 * 
 */

public class UriKey2 : IEquatable<UriKey>
{
	public Uri Key { get; } = null!;
	public Uri?[] Generics { get; } = null!;
	public SemanticVersion Version { get; } = null!;
	public bool Equals(UriKey? other) => other is not null && Uri.Equals(Key, other.Key);
	public override bool Equals(object? obj)
	{
		var identifier = obj as UriKey;
		return identifier is not null && Equals(identifier);
	}
	public override int GetHashCode() => Key.GetHashCode();
}
[JsonConverter(typeof(UriKeyJsonConverter))]
public class UriKey : IEquatable<UriKey>//, IComparable, IComparable<UriKey>
{
	const string RequiredParameterPrefix = "__";
	internal const char ParameterSeparator = '.'; // Valid values: . ! * ( )
	internal const string InterfacesParameterName = RequiredParameterPrefix + "interfaces";
	internal const string GenericsParameterName = "generics";
	internal const string BasesParameterName = RequiredParameterPrefix + "bases";
	public const string FuxionBaseUri = "https://meta.fuxion.dev/";
	public const string FuxionSystemTypesBaseUri = FuxionBaseUri+"system/";

	public UriKey(
#if !NETSTANDARD2_0 && !NET462
		[ConstantExpected]
#endif
		string key)
	{
		(Key, FullUri, Bases, Generics, Interfaces, Version) = ValidateAndNormalizeUri(new(key), false);
	}
	public UriKey(Uri uri)
	{
		(Key, FullUri, Bases, Generics, Interfaces, Version) = ValidateAndNormalizeUri(uri, false);
	}
	internal UriKey(Uri uri, bool allowReservedParameters)
	{
		(Key, FullUri, Bases, Generics, Interfaces, Version) = ValidateAndNormalizeUri(uri, allowReservedParameters);
	}
	
	public Uri FullUri { get; }
	public Uri Key { get; }
	public Uri[] Bases { get; }
	public Uri?[] Generics { get; }
	public Uri[] Interfaces { get; }
	public SemanticVersion Version { get; }
	public Dictionary<string, string?> Parameters { get; } = new();
	internal static (Uri Key, Uri Uri, Uri[] Bases, Uri?[] Generics, Uri[] Interfaces, SemanticVersion Version) ValidateAndNormalizeUri(Uri uri, bool allowReservedParameters)
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
		// foreach (var key in pars.AllKeys.RemoveNulls())
		// {
		// 	Parameters
		// }
		if (!allowReservedParameters)
		{
			if (pars.AllKeys.Any(key => key == InterfacesParameterName)) throw new UriKeyParameterException($"Neither parameter in query can be named '{InterfacesParameterName}'");
			if (pars.AllKeys.Any(key => key == GenericsParameterName)) throw new UriKeyParameterException($"Neither parameter in query can be named '{GenericsParameterName}'");
			if (pars.AllKeys.Any(key => key == BasesParameterName)) throw new UriKeyParameterException($"Neither parameter in query can be named '{BasesParameterName}'");
		}

		// Extract generics
		List<Uri?> generics = new();
		var genericsBase64 = pars[GenericsParameterName]
			?.Split(ParameterSeparator);
		if(genericsBase64 is not null)
			foreach(var generic in genericsBase64)
				generics.Add(string.IsNullOrWhiteSpace(generic) ? null : new(Encoding.UTF8.GetString(generic.FromBase64UrlString())));
		
		// Extract interfaces
		List<Uri> interfaces = new();
		var interfacesBase64 = pars[InterfacesParameterName]
			?.Split(ParameterSeparator);
		if(interfacesBase64 is not null)
			foreach(var @interface in interfacesBase64)
				interfaces.Add(new(Encoding.UTF8.GetString(@interface.FromBase64UrlString())));
		
		// Extract key chain
		List<Uri> keyChain = new();
		var keyChainBase64 = pars[BasesParameterName]
			?.Split(ParameterSeparator);
		if(keyChainBase64 is not null)
			foreach(var key in keyChainBase64)
				keyChain.Add(new(Encoding.UTF8.GetString(key.FromBase64UrlString())));
		
		UriBuilder ub = new(currentUri);
		// Create key Uri
		Dictionary<string,string?> newQuery = new();
		foreach (var key in pars.AllKeys.RemoveNulls().Where(k => !k.StartsWith(RequiredParameterPrefix)))
		{
			newQuery[key] = pars[key];
		}
		ub.Query = newQuery.Aggregate("?", (c, a) => c + (c == "?" ? "" : "&") + a.Key + "=" + a.Value).TrimEnd('?');
		// If the source uri is relative, return it
		if (!uri.IsAbsoluteUri)
		{
			// // Create key Uri
			// Dictionary<string,string?> newQuery = new();
			// foreach (var key in pars.AllKeys.RemoveNulls().Where(k => !k.StartsWith(RequiredParameterPrefix)))
			// {
			// 	newQuery[key] = pars[key];
			// }
			// ub.Query = newQuery.Aggregate("?", (c, a) => c + (c == "?" ? "" : "&") + a.Key + "=" + a.Value).TrimEnd('?');
			return (ub.Uri, uri, Array.Empty<Uri>(), generics.ToArray(), interfaces.ToArray(), version);
		}

		// Remove user info and default port
		ub.UserName = null;
		if (ub.Uri.IsDefaultPort) ub.Port = -1;
		currentUri = ub.Uri;
		
		// // Create key Uri
		// {
		// 	Dictionary<string,string?> newQuery = new();
		// 	foreach (var key in pars.AllKeys.RemoveNulls().Where(k => !k.StartsWith(RequiredParameterPrefix)))
		// 	{
		// 		newQuery[key] = pars[key];
		// 	}
		// 	ub.Query = newQuery.Aggregate("?", (c, a) => c + (c == "?" ? "" : "&") + a.Key + "=" + a.Value).TrimEnd('?');
		// }
		return (ub.Uri, currentUri, keyChain.ToArray(), generics.ToArray(), interfaces.ToArray(), version);
	}
	public override string ToString() => Key.ToString();
	
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
	public bool Equals(UriKey? other) => other is not null && Uri.Equals(Key, other.Key);
	public override bool Equals(object? obj)
	{
		var identifier = obj as UriKey;
		return identifier is not null && Equals(identifier);
	}
	public override int GetHashCode() => Key.GetHashCode();
}