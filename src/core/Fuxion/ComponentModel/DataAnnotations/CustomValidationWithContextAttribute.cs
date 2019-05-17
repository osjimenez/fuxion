using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.ComponentModel.DataAnnotations
{
	public class CustomValidationWithContextAttribute : ValidationAttribute
	{
		public CustomValidationWithContextAttribute(Type type, string methodName)
		{
			method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

			if (method != null)
			{
				if (method.ReturnType == typeof(ValidationResult))
				{
					var pars = method.GetParameters();
					if (pars.Count() != 2 || pars.Last().ParameterType != typeof(ValidationContext))
						throw new ArgumentException($"Method '{methodName}' in type '{type.Name}' specified for this '{nameof(CustomValidationWithContextAttribute)}' must has 2 parameters. First must be of type of property and second must be '{nameof(ValidationContext)}'");
				}
				else throw new ArgumentException($"Method '{methodName}' in type '{type.Name}' specified for this '{nameof(CustomValidationWithContextAttribute)}' must return '{nameof(ValidationResult)}'");
			}
			else throw new ArgumentException($"Method '{methodName}' in type '{type.Name}' specified for this '{nameof(CustomValidationWithContextAttribute)}' was not found. This method must be public and static.");
		}

		MethodInfo method;

		public override bool RequiresValidationContext => true;

		protected override ValidationResult IsValid(object value, ValidationContext context)
		{
			var res = (ValidationResult)method.Invoke(null, new object[] { value, context });
			return res;
		}
	}
}
