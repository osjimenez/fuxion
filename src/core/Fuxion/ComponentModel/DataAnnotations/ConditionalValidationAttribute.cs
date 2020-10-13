using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ConditionalValidationAttribute : Attribute
	{
		public ConditionalValidationAttribute(Type type, string method)
		{
			Type = type;
			Method = method;

			var meth = type.GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
			if (meth != null)
			{
				if (meth.ReturnType == typeof(bool))
				{
					if (meth.GetParameters().Any())
						throw new ArgumentException($"Method '{method}' in type '{type.Name}' specified for this '{nameof(ConditionalValidationAttribute)}' must not has any parameter");
				}
				else throw new ArgumentException($"Method '{method}' in type '{type.Name}' specified for this '{nameof(ConditionalValidationAttribute)}' must return bool");
			}
			else throw new ArgumentException($"Method '{method}' in type '{type.Name}' specified for this '{nameof(ConditionalValidationAttribute)}' was not found. This method must be public and non static.");
		}
		public Type Type { get; }
		public string Method { get; }

		internal bool Check(object instance)
		{
			var met = instance.GetType().GetMethod(Method);
			if (met == null) throw new InvalidOperationException($"Method '{Method}' not found");
			var res = met.Invoke(instance, Array.Empty<object>());
			if (res is bool resBool)
				return resBool;
			throw new InvalidOperationException($"The method '{Method}' doesn't resutrn a bool type");
		}
	}
}
