using System.Collections;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{
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
					if (pars.Count() != 2)
						throw new ArgumentException($"Method '{comparassionMethodName}' in type '{type.Name}' specified for this '{nameof(EnsureNoDuplicatesAttribute)}' must has 2 parameters. Both of them must be of type of property.");
				}
				else throw new ArgumentException($"Method '{comparassionMethodName}' in type '{type.Name}' specified for this '{nameof(EnsureNoDuplicatesAttribute)}' must return 'bool'.");
			}
			else throw new ArgumentException($"Method '{comparassionMethodName}' in type '{type.Name}' specified for this '{nameof(EnsureNoDuplicatesAttribute)}' was not found. This method must be public and static.");
		}
		Type type;
		MethodInfo? method;
		string? lastDuplicateValue;
		public override string FormatErrorMessage(string name)
		{
			return string.Format(ErrorMessageString, name, lastDuplicateValue);
		}
		public override bool IsValid(object? value)
		{
			if (value is IList list)
			{
				if (list.Count < 2) return true;
				for (int i = 1; i < list.Count; i++)
				{
					if ((bool?)method?.Invoke(null, new[] { list[i - 1], list[i] }) ?? false)
					{
						lastDuplicateValue = list[i]?.ToString();
						return false;
					}
				}
				return true;
			}
			return false;
		}

		//public override bool RequiresValidationContext => true;

		//protected override ValidationResult IsValid(object value, ValidationContext context)
		//{
		//	if (value is IList list)
		//	{
		//		if (list.Count < 2) return ValidationResult.Success;
		//		for (int i = 1; i < list.Count; i++)
		//		{
		//			if ((bool)method.Invoke(null, new[] { list[i - 1], list[i] }))
		//			{
		//				lastDuplicateValue = list[i].ToString();
		//				return new ValidationResult(string.Format(ErrorMessageString, context.DisplayName, list[i]));
		//			}
		//		}
		//		return ValidationResult.Success;
		//	}
		//	return new ValidationResult("Value isn't IList");
		//}
	}
}
