using Fuxion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public static class Extensions
    {
        #region Disposable
        public static IDisposable AsDisposable<T>(this T me, Action<T> actionOnDispose) { return new DisposableEnvelope<T>(me, actionOnDispose); }
        class DisposableEnvelope<T> : IDisposable
        {
            public DisposableEnvelope(T obj, Action<T> actionOnDispose)
            {
                this.action = actionOnDispose;
                this.obj = obj;
            }
            T obj;
            Action<T> action;
            void IDisposable.Dispose() { action(obj); }
        }
        #endregion
        #region Json
        public static string ToJson(this object me) { return JsonConvert.SerializeObject(me); }
        public static T FromJson<T>(this string me) { return (T)JsonConvert.DeserializeObject(me, typeof(T)); }
        public static object FromJson(this string me, Type type) { return JsonConvert.DeserializeObject(me, type); }
        public static T CloneWithJson<T>(this T me) { return FromJson<T>(me.ToJson()); } //return (T)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(me, me.GetType(), new JsonSerializerSettings()), me.GetType());
        #endregion
        #region Reflections
        public static string GetFullNameWithAssemblyName(this Type me) { return $"{me.AssemblyQualifiedName.Split(',').Take(2).Aggregate("", (a, n) => a + ", " + n, a => a.Trim(' ', ','))}"; }
        public static string GetSignature(this MethodBase method,
            bool includeAccessModifiers = false,
            bool includeReturn = false,
            bool includeDeclarinType = true,
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
            if (includeDeclarinType)
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
                    res.Append(parametersFunction(useFullNames,includeParametersNames,method, parametersFunctionArguments));
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
        public static string GetSignature(this Type type, bool useFullNames)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                return nullableType.GetSignature(useFullNames) + "?";
            var typeName = useFullNames && !string.IsNullOrWhiteSpace(type.FullName) ? type.FullName : type.Name;
            if (!type.GetTypeInfo().IsGenericType)
                switch (type.Name)
                {
                    case "String": return "string";
                    case "Int32": return "int";
                    case "Int64": return "long";
                    case "Decimal": return "decimal";
                    case "Object": return "object";
                    case "Void": return "void";
                    default:
                        {
                            return typeName;
                        }
                }

            var sb = new StringBuilder(typeName.Substring(0, typeName.IndexOf('`')));
            sb.Append('<');
            var first = true;
            foreach (var t in type.GenericTypeArguments)
            {
                if (!first)
                    sb.Append(',');
                sb.Append(GetSignature(t, useFullNames));
                first = false;
            }
            sb.Append('>');
            return sb.ToString();
        }
        #endregion
        #region Expressions
        //public static string GetMemberName<T>(this object me, Expression<Func<T>> expression)
        //{
        //    return expression.GetMemberName();
        //}
        //public static string GetMemberName<T>(this Expression<Func<T>> expression)
        //{
        //    if (expression.NodeType != ExpressionType.Lambda)
        //        throw new ArgumentException("La expresión debe ser una lambda", "expression");
        //    if (expression.Body is MemberExpression)
        //        return (expression.Body as MemberExpression).Member.Name;
        //    else if (expression.Body is UnaryExpression)
        //        return ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member.Name;
        //    else
        //        throw new ArgumentException("La expresión lambda debe ser de tipo 'MemberExpression' o 'UnaryExpression'.");
        //}
        //public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T>> expression)
        //{
        //    if (expression.NodeType != ExpressionType.Lambda)
        //        throw new ArgumentException("La expresión debe ser una lambda", "expression");
        //    if (expression.Body is MemberExpression)
        //    {
        //        var mem = (expression.Body as MemberExpression).Member;
        //        if (mem is PropertyInfo)
        //            return mem as PropertyInfo;
        //        throw new ArgumentException("La expresión lambda no hace referencia a un miembro de propiedad.");
        //    }
        //    else if (expression.Body is UnaryExpression)
        //    {
        //        var mem = ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member;
        //        if (mem is PropertyInfo)
        //            return mem as PropertyInfo;
        //        throw new ArgumentException("La expresión lambda no hace referencia a un miembro de propiedad.");
        //    }
        //    else
        //        throw new ArgumentException("La expresión lambda debe ser de tipo 'MemberExpression' o 'UnaryExpression'.");
        //}
        #endregion
        #region MemberInfo
        ///// <summary>
        ///// Recupera un atributo personalizado aplicado a un miembro de un tipo.
        ///// </summary>
        ///// <typeparam name="TAttribute">Tipo del atributo personalizado que se va a recuperar.</typeparam>
        ///// <param name="member">Miembro para el cual se recuperará el atributo personalizado.</param>
        ///// <param name="inherit">Si es true, especifica que se busquen también los atributos personalizados de los antecesores.</param>
        ///// <param name="exceptionIfNotFound">Si es true se lanzará una excepción <see cref="AttributeNotFoundException"/> en caso de no encontrarse el atributo personalizado.</param>
        ///// <param name="exceptionIfMoreThanOne">Si es true se lanzará una excepción <see cref="AttributeMoreThanOneException"/> en caso de encontrarse el atributo personalizado más de una vez.</param>
        ///// <returns></returns>
        //public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = true, bool exceptionIfNotFound = true, bool exceptionIfMoreThanOne = true) where TAttribute : Attribute
        //{
        //    var objAtts = member.GetCustomAttributes(typeof(TAttribute), inherit);
        //    var atts = objAtts != null ? objAtts.Cast<TAttribute>() : null;
        //    if (exceptionIfMoreThanOne && atts != null && atts.Count() > 1)
        //        throw new AttributeMoreThanOneException(member, typeof(TAttribute));
        //    var att = atts != null ? atts.FirstOrDefault() : null;
        //    if (exceptionIfNotFound && att == null)
        //        throw new AttributeNotFoundException(member, typeof(TAttribute));
        //    return att;
        //}
        //public static bool HasCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = true, bool exceptionIfMoreThanOne = true) where TAttribute : Attribute
        //{
        //    var att = member.GetCustomAttribute<TAttribute>(inherit, false, exceptionIfMoreThanOne);
        //    return att != null;
        //}
        ///// <summary>
        ///// Excepción que se produce cuando no puede encontrarse un determinado atributo personalizado un un miembro de un tipo.
        ///// </summary>
        //public class AttributeNotFoundException : FuxionException
        //{
        //    /// <summary>
        //    /// Obtiene el miembro para el cual no se ha encontrado el atributo personalizado.
        //    /// </summary>
        //    public MemberInfo Member { get; private set; }
        //    /// <summary>
        //    /// Obtiene el tipo de atributo personalizado que no ha podido ser encontrado.
        //    /// </summary>
        //    public Type AttributeType { get; private set; }
        //    /// <summary>
        //    /// Inicializa una nueva instancia de la clase <see cref="AttributeNotFoundException"/>.
        //    /// </summary>
        //    /// <param name="member">Miembro sobre el que se ha buscado el atributo personalizado.</param>
        //    /// <param name="attributeType">Tipo del atributo personalizado que se ha buscado.</param>
        //    public AttributeNotFoundException(MemberInfo member, Type attributeType)
        //        : base(
        //            //(member.MemberType == MemberTypes.TypeInfo
        //            //    ? "El tipo '" + member.Name + "' "
        //                //: 
        //              "El miembro '" + member.Name + "' del tipo '" + member.DeclaringType?.Name + "' "
        //              //)
        //                + "no esta adornado con el atributo '" + attributeType.Name + "'."



        //            //            "El miembro '" + member.Name + "' del tipo '" + member.DeclaringType.Name +
        //            //					"' no esta adornado con el atributo '" + attributeType.Name + "'."
        //            )
        //    {
        //        Member = member;
        //        AttributeType = attributeType;
        //    }
        //}
        ///// <summary>
        ///// Excepción que se produce cuando se encuentra un determinado atributo personalizado un un miembro de un tipo más de una vez.
        ///// </summary>
        //public class AttributeMoreThanOneException : FuxionException
        //{
        //    /// <summary>
        //    /// Obtiene el miembro para el cual se ha encontrado el atributo personalizado más de una vez.
        //    /// </summary>
        //    public MemberInfo Member { get; private set; }
        //    /// <summary>
        //    /// Obtiene el tipo de atributo personalizado que ha sido encontrado más de una vez.
        //    /// </summary>
        //    public Type AttributeType { get; private set; }
        //    /// <summary>
        //    /// Inicializa una nueva instancia de la clase <see cref="AttributeMoreThanOneException"/>.
        //    /// </summary>
        //    /// <param name="member">Miembro sobre el que se ha buscado el atributo personalizado.</param>
        //    /// <param name="attributeType">Tipo del atributo personalizado que se ha buscado.</param>
        //    public AttributeMoreThanOneException(MemberInfo member, Type attributeType)
        //        : base("El miembro '" + member.Name + "' del tipo '" + member.DeclaringType?.Name +
        //                "' no esta adornado con el atributo '" + attributeType.Name + "'.")
        //    {
        //        Member = member;
        //        AttributeType = attributeType;
        //    }
        //}
        #endregion
    }
}
