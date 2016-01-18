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
	}
}
