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
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type typeInfo)
        => GetAll(typeInfo, ti => ti.GetAllConstructors());

        public static IEnumerable<EventInfo> GetAllEvents(this Type typeInfo)
            => GetAll(typeInfo, ti => ti.GetAllEvents());

        public static IEnumerable<FieldInfo> GetAllFields(this Type typeInfo)
            => GetAll(typeInfo, ti => ti.GetAllFields());

        public static IEnumerable<MemberInfo> GetAllMembers(this Type typeInfo)
            => GetAll(typeInfo, ti => ti.GetAllMembers());

        public static IEnumerable<MethodInfo> GetAllMethods(this Type typeInfo)
            => GetAll(typeInfo, ti => ti.GetAllMethods());

        public static IEnumerable<TypeInfo> GetAllNestedTypes(this Type typeInfo)
            => GetAll(typeInfo, ti => ti.GetAllNestedTypes());

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type typeInfo)
            => GetAll(typeInfo, ti => ti.GetAllProperties());

        private static IEnumerable<T> GetAll<T>(Type typeInfo, Func<Type, IEnumerable<T>> accessor)
        {
            while (typeInfo != null)
            {
                foreach (var t in accessor(typeInfo))
                {
                    yield return t;
                }

                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }
        }
        public static bool IsNullable(this Type me)
        {
            return me.IsClass || me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
