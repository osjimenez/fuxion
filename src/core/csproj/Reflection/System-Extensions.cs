using Fuxion;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = true, bool exceptionIfNotFound = true, bool exceptionIfMoreThanOne = true) where TAttribute : Attribute {
		    var objAtts = member.GetCustomAttributes(typeof (TAttribute), inherit);
            var atts = objAtts != null ? objAtts.Cast<TAttribute>() : null;
			if (exceptionIfMoreThanOne && atts != null && atts.Count() > 1)
				throw new AttributeMoreThanOneException(member, typeof(TAttribute));
			var att = atts != null ? atts.FirstOrDefault() : null;
			if (exceptionIfNotFound && att == null)
				throw new AttributeNotFoundException(member, typeof(TAttribute));
			return att;
		}
        public static bool HasCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = true, bool exceptionIfMoreThanOne = true) where TAttribute : Attribute
		{
			var att = member.GetCustomAttribute<TAttribute>(inherit, false, exceptionIfMoreThanOne);
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
            var res = new StringBuilder();
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
                res.Append(((MethodInfo)method).ReturnType.GetSignature(useFullNames) + " ");
            // Method name
            if (includeDeclaringType)
                res.Append(method.DeclaringType.GetSignature(useFullNames) + ".");
            res.Append(method.Name);
            // Generics arguments
            if (method.IsGenericMethod)
            {
                res.Append("<");
                var genericArgs = method.GetGenericArguments();
                for (var i = 0; i < genericArgs.Length; i++)
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
                    var pars = method.GetParameters();
                    for (var i = 0; i < pars.Length; i++)
                    {
                        var par = pars[i];
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

        //public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type me, BindingFlags bindingAtt = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) => GetAll(me, ti => ti.GetConstructors(bindingAtt), false);
        //public static IEnumerable<EventInfo> GetAllEvents(this Type me, BindingFlags bindingAtt = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) => GetAll(me, ti => ti.GetEvents(bindingAtt));
        //public static IEnumerable<FieldInfo> GetAllFields(this Type me) => GetAll(me, ti => ti.GetFields());
        //public static IEnumerable<MemberInfo> GetAllMembers(this Type me) => GetAll(me, ti => ti.GetMembers());
        //public static IEnumerable<MethodInfo> GetAllMethods(this Type me) => GetAll(me, ti => ti.GetMethods());
        //public static IEnumerable<Type> GetAllNestedTypes(this Type me) => GetAll(me, ti => ti.GetNestedTypes());
        //public static IEnumerable<PropertyInfo> GetAllProperties(this Type me) => GetAll(me, ti => ti.GetProperties());
        //private static IEnumerable<T> GetAll<T>(Type me, Func<Type, IEnumerable<T>> accessor, bool recursive = true)
        //{
        //    if (recursive)
        //    {
        //        var res = new List<T>();
        //        while (me != null)
        //        {
        //            foreach (var t in accessor(me))
        //            {
        //                res.Add(t);
        //            }
        //            me = me.BaseType?.GetTypeInfo();
        //        }
        //        return res;
        //    }
        //    else return accessor(me);
        //}

        public static bool IsNullable(this Type me)
        {
            return me.IsClass || me.IsInterface || me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static object GetDefaultValue(this Type t)
        {
            if (t.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
            else
                return null;
        }


    }
}
