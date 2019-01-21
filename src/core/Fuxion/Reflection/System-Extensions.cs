using Fuxion.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reflection
{
	public static class MemberInfoExtensions
	{
		/// <summary>
		/// Recupera un atributo personalizado aplicado a un miembro de un tipo.
		/// </summary>
		/// <typeparam name="TAttribute">Tipo del atributo personalizado que se va a recuperar.</typeparam>
		/// <param name="member">Miembro para el cual se recuperará el atributo personalizado.</param>
		/// <param name="inherit">Si es true, especifica que se busquen también los atributos personalizados de los antecesores.</param>
		/// <param name="exceptionIfNotFound">Si es true se lanzará una excepción <see cref="AttributeNotFoundException"/> en caso de no encontrarse el atributo personalizado.</param>
		/// <param name="exceptionIfMoreThanOne">Si es true se lanzará una excepción <see cref="AttributeMoreThanOneException"/> en caso de encontrarse el atributo personalizado más de una vez.</param>
		/// <returns></returns>
		public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = true, bool exceptionIfNotFound = true, bool exceptionIfMoreThanOne = true) where TAttribute : Attribute
		{
			object[] objAtts = member.GetCustomAttributes(typeof(TAttribute), inherit);
			IEnumerable<TAttribute> atts = objAtts != null ? objAtts.Cast<TAttribute>() : null;
			if (exceptionIfMoreThanOne && atts != null && atts.Count() > 1)
				throw new AttributeMoreThanOneException(member, typeof(TAttribute));
			TAttribute att = atts != null ? atts.FirstOrDefault() : null;
			if (exceptionIfNotFound && att == null)
				throw new AttributeNotFoundException(member, typeof(TAttribute));
			return att;
		}
		public static bool HasCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = true, bool exceptionIfMoreThanOne = true) where TAttribute : Attribute
		{
			TAttribute att = member.GetCustomAttribute<TAttribute>(inherit, false, exceptionIfMoreThanOne);
			return att != null;
		}
		public static string GetSignature(this MethodBase method,
			bool includeAccessModifiers = false,
			bool includeReturn = false,
			bool includeDeclaringType = true,
			bool useFullNames = false,
			bool includeParameters = true,
			bool includeParametersNames = false,
			Func<bool, bool, MethodBase, object, string> parametersFunction = null,
			object parametersFunctionArguments = null
			)
		{
			StringBuilder res = new StringBuilder();
			// Access modifiers
			if (includeAccessModifiers)
			{
				if (method.IsPublic)
					res.Append("public ");
				else if (method.IsPrivate)
					res.Append("private ");
				else if (method.IsAssembly)
					res.Append("internal ");
				if (method.IsFamily)
					res.Append("protected ");
				if (method.IsStatic)
					res.Append("static ");
				if (method.IsVirtual)
					res.Append("virtual ");
				if (method.IsAbstract)
					res.Append("abstract ");
			}
			// Return type
			if (includeReturn)
			{
				if (method is MethodInfo mi)
					res.Append(mi.ReturnType.GetSignature(useFullNames) + " ");
				else
					res.Append("<ctor> ");
			}
			// Method name
			if (includeDeclaringType)
				res.Append(method.DeclaringType.GetSignature(useFullNames) + ".");
			res.Append(method.Name);
			// Generics arguments
			if (method.IsGenericMethod)
			{
				res.Append("<");
				Type[] genericArgs = method.GetGenericArguments();
				for (int i = 0; i < genericArgs.Length; i++)
					res.Append((i > 0 ? ", " : "") + genericArgs[i].GetSignature(useFullNames));
				res.Append(">");
			}
			// Parameters
			if (includeParameters)
			{
				res.Append("(");
				if (parametersFunction != null)
				{
					res.Append(parametersFunction(useFullNames, includeParametersNames, method, parametersFunctionArguments));
				}
				else
				{
					ParameterInfo[] pars = method.GetParameters();
					for (int i = 0; i < pars.Length; i++)
					{
						ParameterInfo par = pars[i];
						if (i == 0 && method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
							res.Append("this ");
						if (par.ParameterType.IsByRef)
							res.Append("ref ");
						else if (par.IsOut)
							res.Append("out ");
						res.Append(par.ParameterType.GetSignature(useFullNames));
						if (includeParametersNames)
							res.Append(" " + par.Name);
						if (i < pars.Length - 1)
							res.Append(", ");
					}
				}
				res.Append(")");
			}
			return res.ToString();
		}
	}
}
