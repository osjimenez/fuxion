using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fuxion.ComponentModel.DataAnnotations
{
    public class CustomValidationWithContextAttribute : ValidationAttribute
    {
        public CustomValidationWithContextAttribute(Type type, string method)
        {
            Type = type;
            Method = method;

            var meth = type.GetMethod(method, BindingFlags.Static | BindingFlags.Public);
            if (meth != null)
            {
                if (meth.ReturnType == typeof(ValidationResult))
                {
                    var pars = meth.GetParameters();
                    if (pars.Count() != 2 || pars.Last().ParameterType != typeof(ValidationContext))
                            throw new ArgumentException($"Method '{method}' in type '{type.Name}' specified for this '{nameof(ConditionalValidationAttribute)}' must has 2 parameters. First must be of type of property and second must be '{nameof(ValidationContext)}'");
                }
                else throw new ArgumentException($"Method '{method}' in type '{type.Name}' specified for this '{nameof(ConditionalValidationAttribute)}' must return '{nameof(ValidationResult)}'");
            }
            else throw new ArgumentException($"Method '{method}' in type '{type.Name}' specified for this '{nameof(ConditionalValidationAttribute)}' was not found. This method must be public and non static.");
        }
        public Type Type { get; }
        public string Method { get; }
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var res =  (ValidationResult)context.ObjectInstance.GetType().GetMethod(Method, BindingFlags.Static | BindingFlags.Public).Invoke(context.ObjectInstance, new object[] { value, context });
            return res;
        }
        public override string FormatErrorMessage(string name)
        {
            var tt = ErrorMessageString;
            return base.FormatErrorMessage(name);
        }
        public override bool RequiresValidationContext => true;
    }
}
