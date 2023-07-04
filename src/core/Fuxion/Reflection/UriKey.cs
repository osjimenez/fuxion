using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Web;

namespace Fuxion.Reflection;

public class UriKey
{
	public const string INTERFACE_PARAMETER_NAME = "__interface";
	public const string BASE_PARAMETER_NAME = "__base";

	public UriKey(string key)
	{
		Uri = ValidateAndNormalizeUri(new(key), false);
	}
	public UriKey(Uri uri)
	{
		Uri = ValidateAndNormalizeUri(uri, false);
	}
	internal UriKey(Uri uri, bool allowReservedParameters)
	{
		Uri = ValidateAndNormalizeUri(uri, allowReservedParameters);
	}
	
	public Uri Uri { get; }
	internal static Uri ValidateAndNormalizeUri(Uri uri, bool allowReservedParameters)
	{
		// If uri is relative, make it absolute only to process it
		var currentUri = uri;
		if (!uri.IsAbsoluteUri && Uri.TryCreate(new("https://domain.com"), uri, out var tempUri)) currentUri = tempUri;
		
		// Validate that the last segment is a valid semantic version pattern
		var lastSegment = currentUri.Segments.LastOrDefault()
			?? throw new UriKeySemanticVersionException($"Uri must be a last segment with a semantic version valid pattern");
		if (!SemanticVersion.TryParse(lastSegment, out var _)) throw new UriKeySemanticVersionException($"The last segment '{lastSegment}' isn't a valid semantic version pattern");
		
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
		if (!uri.IsAbsoluteUri) return uri;
		
		// Use UriBuilder to normalize the uri
		UriBuilder ub = new(currentUri);
		ub.UserName = null;
		if (ub.Uri.IsDefaultPort) ub.Port = -1;
		currentUri = ub.Uri;
		
		return currentUri;
	}
	public override string ToString() => Uri.ToString();
}