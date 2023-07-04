using System.Reflection;
using Fuxion.Reflection;

namespace Fuxion;

public static class UriKeyExtensions
{
	public static UriKey GetUriKey(this Type me)
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
					throw new UriKeyBypassedException($"Type '{me.Name}' is bypassed and cannot have a UriKey associated");
				// If any except last in the chain is seal, throw
				var @sealed = list.SkipLast(1).FirstOrDefault(t=>t?.Uri?.IsSealed ?? false);
				if(@sealed is not null)
					throw new UriKeySealedException($"Type '{@sealed.Value.Type.Name}' is sealed in '{nameof(UriKeyAttribute)}'.");
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