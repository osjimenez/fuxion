using System.Text;
using System.Web;
using Fuxion.Collections.Generic;
using Fuxion.Reflection;

namespace Fuxion;

public static class UriKeyExtensions
{
	public static UriKey GetUriKey(this Type me, bool includeSystemTypes = true)
	{
		if (includeSystemTypes && SystemUriKeys.TryGetFor(me, out var uriKey)) return uriKey;
		var currentType = (Type?)me;
		List<(UriKeyAttribute? Att, UriKeyBypassAttribute? Bypass, Type Type)> inheritanceChain = new();
		while (currentType is not null)
		{
			var att = currentType.GetCustomAttribute<UriKeyAttribute>(false, false, true);
			var bypassAtt = currentType.GetCustomAttribute<UriKeyBypassAttribute>(false, false, true);
			inheritanceChain.Add((att, bypassAtt, currentType));
			currentType = currentType.BaseType;
		}
		inheritanceChain.Reverse();
		List<List<(Uri? Uri, bool? IsSealed, bool? IsReset, UriKeyBypassAttribute? Bypass, Type Type)>> keyList = new();
		List<(Uri? Uri, bool? IsSealed, bool? IsReset, UriKeyBypassAttribute? Bypass, Type Type)> uriList = new();
		foreach (var (att, bypassAtt, type) in inheritanceChain)
		{
			switch (att)
			{
				// If set IsReset but uri is not absolute, throw
				case { IsReset: true, Uri.IsAbsoluteUri: false }: throw new UriKeyResetException($"When you wants to reset the chain, you need set an absolute uri");
				// If not reset, is absolute uri and isn't the first key, throw 
				case { IsReset: false, Uri.IsAbsoluteUri: true }
					when keyList.Count > 0: throw new UriKeyResetException($"When you wants to reset the chain, you need set '{nameof(UriKeyAttribute.IsReset)}' parameter");
			}
			// If is absolute uri, reset the chain and create new key
			if (att?.Uri.IsAbsoluteUri ?? false)
			{
				keyList.Add(uriList);
				uriList = new();
			}
			// Set type generic parameters & interfaces
			Uri? extendedUri = null;
			if (att is not null)
			{
				var ub = att.Uri.IsAbsoluteUri ? new(att.Uri.ToString()) : new UriBuilder("http://domain.com/" + att.Uri);
				var query = HttpUtility.ParseQueryString(ub.Query);
				// Set generic parameters
				if (type.IsGenericType)
				{
					if (type.IsConstructedGenericType)
					{
						query[UriKey.GenericsParameterName] = type.GetGenericArguments()
							.Select(g => Encoding.UTF8.GetBytes(g.GetUriKey(includeSystemTypes).ToString()).ToBase64UrlString())
							.Aggregate("", (c, a) => c + UriKey.ParameterSeparator + a, c => c.Trim(UriKey.ParameterSeparator));
					} else
					{
						query[UriKey.GenericsParameterName] = type.GetGenericArguments()
							.Select(g => "")
							.Skip(1)
							.Aggregate("", (c, a) => c + UriKey.ParameterSeparator + a);
					}
				}
				// Set interface parameters
				var interfaces = type.GetInterfaces()
					.Where(i=>i.HasCustomAttribute<UriKeyAttribute>())
					.ToList();
				if (interfaces.Count > 0)
				{
					query[UriKey.InterfacesParameterName] = interfaces
						.Select(i => Encoding.UTF8.GetBytes(i.GetUriKey(includeSystemTypes).ToString()).ToBase64UrlString())
						.Aggregate("", (c, a) => c + UriKey.ParameterSeparator + a, c => c.Trim(UriKey.ParameterSeparator));
				}
				ub.Query = query.ToString();
				extendedUri = att.Uri.IsAbsoluteUri ? ub.Uri : new Uri("http://domain.com/").MakeRelativeUri(ub.Uri);
			}
			uriList.Add((extendedUri ?? att?.Uri, att?.IsSealed, att?.IsReset, bypassAtt, type));
		}
		if (uriList.Count > 0) keyList.Add(uriList);
		// If first uri of last key isn't absolute uri, throw
		if (!keyList.Last()
			.First()
			.Uri?.IsAbsoluteUri ?? true)
			throw new UriKeyInheritanceException($"'{nameof(UriKeyAttribute)}' inheritance must start with an absolute uri. The type '{me.Name}' couldn't be processed.");
		List<((Uri Uri, Type Type)? Current, (Uri Uri, Type Type)? Base, Exception? Exception)> results = new();
		foreach (var list in keyList)
		{
			try
			{
				// If any of type wasn't adorned with any attribute, throw
				if (list.Any(t => t.Uri == null && t.Bypass == null))
					throw new AttributeNotFoundException(list.First(t => t.Uri == null && t.Bypass == null)
						.Type, typeof(UriKeyAttribute));
				// If any except first in the chain is seal, throw
				var @sealed = list.Skip(1)
					.FirstOrDefault(t => t.IsSealed ?? false);
				if (@sealed.Uri is not null) throw new UriKeySealedException($"Type '{@sealed.Type.Name}' is sealed in '{nameof(UriKeyAttribute)}'.");
				// If first in the chain is bypass, save the exception to add it to the list
				UriKeyBypassedException? bypassedException = null;
				if (list.Last()
					.Bypass is not null)
					bypassedException = new($"Type '{me.Name}' is bypassed and cannot have a '{nameof(UriKey)}' associated.");
				// Create uris
				results.Add(((list.Where(t => t.Bypass == null)
					.Select(t => t.Uri)
					.RemoveNulls()
					.Aggregate((a, c) => new(a, c)), list.Last()
					.Type), (list.First()
					.Uri!, list.First()
					.Type), bypassedException));
			} catch (Exception ex)
			{
				// If exception, add to res
				results.Add((null, null, ex));
			}
		}
		// If last key was fail, throw
		var (lastKey, _, exception) = results.Last();
		if (exception is not null) throw exception;

		if (lastKey is null) throw new InvalidProgramException($"Unexpected fail when process '{nameof(UriKey)}'.");
		var (lastKeyUri, lastKeyType) = lastKey.Value;

		// Get first key as result and the rest as reset chain
		{
			UriBuilder ub = new(lastKeyUri);
			var keyChain = "";
			// var baseCount = 1;
			foreach (var (current, @base) in results.SkipLast(1)
				.Where(r => r.Current?.Uri != null)
				.Select(r => (r.Current!.Value, r.Base)))
			{
				if (current.Uri is null || @base?.Uri is null || @base?.Type is null) throw new InvalidProgramException($"Unexpected fail when process UriKey");
				if (@base.Value.Uri.IsBaseOf(lastKeyUri))
					throw new UriKeyResetException($"The uri '{lastKeyUri}' of the reset type '{lastKeyType.Name}' cannot be based on previous uri '{@base.Value.Uri}' of type '{@base.Value.Type.Name}'");
				keyChain += Encoding.UTF8.GetBytes(current.Uri.ToString())
					.ToBase64UrlString()+UriKey.ParameterSeparator;
			}
			if (!string.IsNullOrWhiteSpace(keyChain))
			{
				var query = HttpUtility.ParseQueryString(ub.Query);
				query[UriKey.BasesParameterName] = keyChain.Trim(UriKey.ParameterSeparator);
				ub.Query = query.ToString();
			}
			return new(ub.Uri, true);
		}
	}
}