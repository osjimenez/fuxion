namespace Fuxion.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class UriKeyAttribute(string key, bool isSealed = true) : Attribute
{
	public Uri Uri { get; } = UriKey.ValidateAndNormalizeUri(new(key, UriKind.RelativeOrAbsolute), false).Uri;
	public bool IsSealed { get; } = isSealed;
}
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class UriKeyBypassAttribute : Attribute { }