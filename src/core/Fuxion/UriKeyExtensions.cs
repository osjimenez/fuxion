using System.Reflection;
using Fuxion;
using Fuxion.Reflection;

public static class UriKeyExtensions
{
	public static UriKey GetUriKey(this Type me)
	{
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
		List<List<(UriKeyAttribute? Att, UriKeyBypassAttribute? Bypass, Type Type)>> keyList = new();
		List<(UriKeyAttribute? Att, UriKeyBypassAttribute? Bypass, Type Type)> uriList = new();
		foreach (var (att, bypassAtt, type) in inheritanceChain)
		{
			switch (att)
			{
				// If set IsReset but uri is not absolute, throw
				case { IsReset: true, Uri.IsAbsoluteUri: false }: throw new UriKeyResetException($"When you wants to reset the chain, you need set an absolute uri");
				// If not reset, is absolute uri and isn't the first key, throw 
				case { IsReset: false, Uri.IsAbsoluteUri: true } when keyList.Count > 0: throw new UriKeyResetException($"When you wants to reset the chain, you need set '{nameof(UriKeyAttribute.IsReset)}' parameter");
			}
			// If is absolute uri, reset the chain and create new key
			if (att?.Uri.IsAbsoluteUri ?? false)
			{
				// if (!att.Uri.IsAbsoluteUri) throw new UriKeyResetException($"When you wants to reset the chain, you need set an absolute uri");
				// uriList.Reverse();
				keyList.Add(uriList);
				uriList = new();
			}
			uriList.Add((att, bypassAtt, type));
		}
		if(uriList.Count > 0 )
			keyList.Add(uriList);
		// If first uri of last key isn't absolute uri, throw
		if (!keyList.Last()
			.First()
			.Att?.Uri.IsAbsoluteUri ?? true)
			throw new UriKeyInheritanceException($"'{nameof(UriKeyAttribute)}' inheritance must start with an absolute uri");
		List<((Uri Uri, Type Type)? Current,(Uri Uri, Type Type)? Base, Exception? Exception)> results = new();
		foreach (var list in keyList)
		{
			try
			{
				// If any of type wasn't adorned with any attribute, throw
				if(list.Any(t => t.Att == null && t.Bypass == null))
					throw new AttributeNotFoundException(list.First(t => t.Att == null && t.Bypass == null).Type, typeof(UriKeyAttribute));
				// If any except first in the chain is seal, throw
				var @sealed = list.Skip(1).FirstOrDefault(t=>t.Att?.IsSealed ?? false);
				if(@sealed.Att is not null)
					throw new UriKeySealedException($"Type '{@sealed.Type.Name}' is sealed in '{nameof(UriKeyAttribute)}'.");
				// If first in the chain is bypass, save the exception to add it to the list
				UriKeyBypassedException? bypassedException = null;
				if (list
					.Last()
					.Bypass is not null)
					bypassedException = new($"Type '{me.Name}' is bypassed and cannot have a UriKey associated");
				// Create uris
				results.Add(((list
					.Where(t => t.Bypass == null)
					.Select(t => t.Att?.Uri)
					.RemoveNulls()
					.Aggregate((a, c) => new(a, c)), list.Last()
					.Type), (list.First()
					.Att!.Uri, list.First()
					.Type), bypassedException));
			} catch (Exception ex)
			{
				// If exception, add to res
				results.Add((null, null, ex));
			}
		}
		// If last key was fail, throw
		var (lastKey, _, exception) = results.Last();
		if (exception is not null)
			throw exception;
		
		if(lastKey is null)throw new InvalidProgramException($"Unexpected fail when process UriKey");
		var (lastKeyUri, lastKeyType) = lastKey.Value;
		
		// Get first key as result and the rest as reset chain
		UriBuilder ub = new(lastKeyUri);
		var baseCount = 1;
		foreach (var (uri, baseUri, baseType) in results.SkipLast(1)
			.Where(r => r.Current?.Uri != null)
			.Select(r => (r.Current?.Uri, r.Base?.Uri, r.Base?.Type)))
		{
			if (uri is null || baseUri is null || baseType is null) throw new InvalidProgramException($"Unexpected fail when process UriKey");
			if (baseUri.IsBaseOf(lastKeyUri)) 
				throw new UriKeyResetException($"The uri '{lastKeyUri}' of the reset type '{lastKeyType.Name}' cannot be based on previous uri '{baseUri}' of type '{baseType.Name}'");
			if (string.IsNullOrWhiteSpace(ub.Query))
				ub.Query = $"?{UriKey.BaseParameterNamePrefix}{baseCount}={Uri.EscapeDataString(uri.ToString())}";
			else
				ub.Query += $"&{UriKey.BaseParameterNamePrefix}{baseCount}={Uri.EscapeDataString(uri.ToString())}";
			baseCount++;
		}

		return new(ub.Uri, true);
	}
}