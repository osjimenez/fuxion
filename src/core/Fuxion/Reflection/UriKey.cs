using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Reflection;
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
			?? throw new UriKeyException($"Uri must be a last segment with a semantic version valid pattern");
		if (!SemanticVersion.TryParse(lastSegment, out var _)) throw new UriKeyException($"The last segment '{lastSegment}' isn't a valid semantic version pattern");
		
		// If the uri is relative, checks that not start with '/'
		if(!uri.IsAbsoluteUri && uri.ToString().StartsWith('/'))
			throw new UriKeyException($"Relative Uri path cannot begins with '/'");
		
		// Validate that hasn't user info
		// Issue when call IsBaseOf and UserInfo differs
		// https://github.com/dotnet/runtime/issues/88265
		// This a BUG that Microsoft must be resolve
		if (!string.IsNullOrWhiteSpace(currentUri.UserInfo)) throw new UriKeyException($"Uri cannot has user info");
		
		// Validate that hasn't fragment
		if (!string.IsNullOrWhiteSpace(currentUri.Fragment)) throw new UriKeyException($"Uri cannot has fragment");
		
		// Validate that hasn't parameters named with reserved names, if not allowed
		var pars = HttpUtility.ParseQueryString(currentUri.Query);
		if (!allowReservedParameters)
		{
			if (pars.AllKeys.Any(key => key == INTERFACE_PARAMETER_NAME)) throw new UriKeyException($"Neither parameter in query can be named '{INTERFACE_PARAMETER_NAME}'");
			if (pars.AllKeys.Any(key => key == BASE_PARAMETER_NAME)) throw new UriKeyException($"Neither parameter in query can be named '{BASE_PARAMETER_NAME}'");
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

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class UriKeyAttribute(string key, bool isSealed = true) : Attribute
{
	public Uri Uri { get; } = GetUri(key);
	public bool IsSealed { get; } = isSealed;
	static Uri GetUri(string key)
	{
		if (Uri.TryCreate(key, UriKind.RelativeOrAbsolute, out var uri))
			return UriKey.ValidateAndNormalizeUri(uri, false);
		throw new UriKeyException($"Uri (relative or absolute) cannot be created from key '{key}'");
	}
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class UriKeyBypassAttribute : Attribute { }
public class UriKeyException(string message) : FuxionException(message);

public static class UriKeyExtensions
{
	public static UriKey? GetUriKey(this Type me)
	{
		List<List<(UriKeyAttribute? Uri, UriKeyBypassAttribute? Bypass, Type Type)?>> keyList = new();
		List<(UriKeyAttribute? Uri, UriKeyBypassAttribute? Bypass, Type Type)?> uriList = new();
		var type = (Type?)me;
		while (type is not null)
		{
			var att = type.GetCustomAttribute<UriKeyAttribute>(false, false, true);
			var bypassAtt = type.GetCustomAttribute<UriKeyBypassAttribute>(false, false, true); 
			uriList.Add((att, bypassAtt, type));
			if (att?.Uri.IsAbsoluteUri ?? false)
			{
				uriList.Reverse();
				keyList.Add(uriList);
				uriList = new();
			}
			type = type.BaseType;
		}
		// If any key list can be made, any absolute uri exist
		if (keyList.Count == 0) throw new AttributeNotFoundException(me, typeof(UriKeyAttribute));
		List<(Uri? Key, Exception? Exception)> res = new();
		foreach (var list in keyList)
		{
			try
			{
				// If any broken type, throw
				var broken = list.FirstOrDefault(t => t?.Uri == null && t?.Bypass == null);
				if (broken is not null) throw new AttributeNotFoundException(broken.Value.Type, typeof(UriKeyAttribute));
				// If last in the chain is bypass, throw
				if (list.RemoveNulls()
					.Last()
					.Bypass is not null)
					throw new UriKeyException($"Type '{me.Name}' is bypassed and cannot have a UriKey associated");
				// If any except last in the chain is seal, throw
				var @sealed = list.SkipLast(1).FirstOrDefault(t=>t?.Uri?.IsSealed ?? false);
				if(@sealed is not null)
					throw new UriKeyException($"Type '{@sealed.Value.Type.Name}' is sealed in '{nameof(UriKeyAttribute)}'.");
				// Remove bypass echelons
				var resList = list.RemoveNulls()
					.ToList()
					.RemoveIf(t => t.Bypass == null);
				// Create UriKey
				res.Add((resList.Select(t => t.Uri?.Uri)
					.RemoveNulls()
					.Aggregate((a, c) => new(a, c)), null));
			} catch (Exception ex)
			{
				// If exception, add to res
				res.Add((null, ex));
			}
		}
		// If first key was fail, throw
		var (result, exception) = res.First();
		if (exception is not null)
			throw exception;
		if (result is null) throw new InvalidProgramException($"Unexpected fail when process UriKey");
		// Get first key as result and the rest as reset chain
		UriBuilder ub = new(result);
		foreach (var uri in res.Skip(1).Where(r=>r.Key != null).Select(r=>r.Key).RemoveNulls())
			if (string.IsNullOrWhiteSpace(ub.Query))
				ub.Query = $"?{UriKey.BASE_PARAMETER_NAME}={Uri.EscapeDataString(uri.ToString())}";
			else
				ub.Query += $"&{UriKey.BASE_PARAMETER_NAME}={Uri.EscapeDataString(uri.ToString())}";
		
		return new(ub.Uri, true);
	}
}
