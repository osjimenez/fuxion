namespace Fuxion;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class UriKeyAttribute(string key, bool isSealed = false, bool isReset = false) : Attribute
{
	public Uri Uri { get; } = UriKey.ValidateAndNormalizeUri(new(key, UriKind.RelativeOrAbsolute), false).Uri;
	public bool IsSealed { get; } = isSealed;
	public bool IsReset { get; } = isReset;
}
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class UriKeyBypassAttribute : Attribute { }