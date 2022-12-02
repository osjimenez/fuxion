using System.Collections;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations;

public class EnsureNoDuplicatesAttribute : ValidationAttribute
{
	public EnsureNoDuplicatesAttribute(Type type, string comparassionMethodName)
	{
		this.type = type;
		method = type.GetMethod(comparassionMethodName, BindingFlags.Static | BindingFlags.Public);
		if (method != null)
		{
			if (method.ReturnType == typeof(bool))
			{
				var pars = method.GetParameters();
				if (pars.Count() != 2) throw new ArgumentException($"Method '{comparassionMethodName}' in type '{type.Name}' specified for this '{nameof(EnsureNoDuplicatesAttribute)}' must has 2 parameters. Both of them must be of type of property.");
			} else
				throw new ArgumentException($"Method '{comparassionMethodName}' in type '{type.Name}' specified for this '{nameof(EnsureNoDuplicatesAttribute)}' must return 'bool'.");
		} else
			throw new ArgumentException($"Method '{comparassionMethodName}' in type '{type.Name}' specified for this '{nameof(EnsureNoDuplicatesAttribute)}' was not found. This method must be public and static.");
	}
	readonly MethodInfo? method;
	readonly Type type;
	string? lastDuplicateValue;
	public override string FormatErrorMessage(string name) => string.Format(ErrorMessageString, name, lastDuplicateValue);
	public override bool IsValid(object? value)
	{
		if (value is IList list)
		{
			if (list.Count < 2) return true;
			for (var i = 1; i < list.Count; i++)
				if ((bool?)method?.Invoke(null, new[]
					 {
						 list[i - 1], list[i]
					 }) ?? false)
				{
					lastDuplicateValue = list[i]?.ToString();
					return false;
				}
			return true;
		}
		return false;
	}
}