using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Fuxion.Reflection;

public static class MemberInfoExtensions
{
	// TODO GetCustomAttribute is now in framework but not with same parameters and behavior
	
	/// <summary>
	///    Retrieves a custom attribute of a specified type that is applied to a specified
	///    member, and optionally inspects the ancestors of that member.
	/// </summary>
	/// <typeparam name="TAttribute">The type of attribute to search for.</typeparam>
	/// <param name="me">The member to inspect.</param>
	/// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
	/// <param name="exceptionIfNotFound">true to throw <see cref="AttributeNotFoundException" /> if custom attribute not found.</param>
	/// <param name="exceptionIfMoreThanOne">true to throw <see cref="AttributeMoreThanOneException" /> if custom attribute found more than once.</param>
	/// <returns></returns>
	public static TAttribute? GetCustomAttribute<TAttribute>(this MemberInfo me,
		bool inherit = true, [DoesNotReturnIf(true)] bool exceptionIfNotFound = false,
		bool exceptionIfMoreThanOne = false) where TAttribute : Attribute
	{
		var objAtts = me.GetCustomAttributes(typeof(TAttribute), inherit);
		var atts = objAtts?.Cast<TAttribute>().ToArray();
		if (exceptionIfMoreThanOne && atts is { Length: > 1 }) throw new AttributeMoreThanOneException(me, typeof(TAttribute));
		var att = atts?.FirstOrDefault();
		if (exceptionIfNotFound && att == null) throw new AttributeNotFoundException(me, typeof(TAttribute));
		return att;
	}
	public static bool HasCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = true, [DoesNotReturnIf(true)] bool exceptionIfMoreThanOne = true)
		where TAttribute : Attribute
		=> member.GetCustomAttribute<TAttribute>(inherit, false, exceptionIfMoreThanOne) is not null;
	
	public static string GetSignature(this MethodBase method,
		bool includeAccessModifiers = false,
		bool includeReturn = false,
		bool includeDeclaringType = true,
		bool useFullNames = false,
		bool includeParameters = true,
		bool includeParametersNames = false,
		Func<bool, bool, MethodBase, object?, string>? parametersFunction = null,
		object? parametersFunctionArguments = null)
	{
		var res = new StringBuilder();
		// Access modifiers
		if (includeAccessModifiers)
		{
			if (method.IsPublic)
				res.Append("public ");
			else if (method.IsPrivate)
				res.Append("private ");
			else if (method.IsAssembly) res.Append("internal ");
			if (method.IsFamily) res.Append("protected ");
			if (method.IsStatic) res.Append("static ");
			if (method.IsVirtual) res.Append("virtual ");
			if (method.IsAbstract) res.Append("abstract ");
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
		if (includeDeclaringType) res.Append(method.DeclaringType?.GetSignature(useFullNames) + ".");
		res.Append(method.Name);
		// Generics arguments
		if (method.IsGenericMethod)
		{
			res.Append("<");
			var genericArgs = method.GetGenericArguments();
			for (var i = 0; i < genericArgs.Length; i++) res.Append((i > 0 ? ", " : "") + genericArgs[i].GetSignature(useFullNames));
			res.Append(">");
		}
		// Parameters
		if (includeParameters)
		{
			res.Append("(");
			if (parametersFunction != null)
				res.Append(parametersFunction(useFullNames, includeParametersNames, method, parametersFunctionArguments));
			else
			{
				var pars = method.GetParameters();
				for (var i = 0; i < pars.Length; i++)
				{
					var par = pars[i];
					if (i == 0 && method.IsDefined(typeof(ExtensionAttribute), false)) res.Append("this ");
					if (par.ParameterType.IsByRef)
						res.Append("ref ");
					else if (par.IsOut) res.Append("out ");
					res.Append(par.ParameterType.GetSignature(useFullNames));
					if (includeParametersNames) res.Append(" " + par.Name);
					if (i < pars.Length - 1) res.Append(", ");
				}
			}
			res.Append(")");
		}
		return res.ToString();
	}
	// TODO Check when folder is null or empty
	public static Stream? GetResourceStream(this Assembly assembly, string folder, string fileName)
	{
		if (assembly.FullName is null) throw new InvalidOperationException("assembly.FullName is null");
		var resourceName = assembly.FullName.Split(',')[0] + "." + folder.Replace("\\", ".").Replace("/", ".") + "." + fileName;
		return new List<string>(assembly.GetManifestResourceNames())
			.Contains(resourceName)
			? assembly.GetManifestResourceStream(resourceName)
			: null;
	}
	// TODO Check when folder is null or empty
	public static string? GetResourceAsString(this Assembly assembly, string folder, string fileName)
	{
		var stream = GetResourceStream(assembly, folder, fileName);
		return stream is null
			? null
			: new StreamReader(stream).ReadToEnd();
	}
}