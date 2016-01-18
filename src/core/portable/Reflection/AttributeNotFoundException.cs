using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Reflection
{
    /// <summary>
    /// Excepción que se produce cuando no puede encontrarse un determinado atributo personalizado un un miembro de un tipo.
    /// </summary>
    public class AttributeNotFoundException : FuxionException
    {
        /// <summary>
        /// Obtiene el miembro para el cual no se ha encontrado el atributo personalizado.
        /// </summary>
        public MemberInfo Member { get; private set; }
        /// <summary>
        /// Obtiene el tipo de atributo personalizado que no ha podido ser encontrado.
        /// </summary>
        public Type AttributeType { get; private set; }
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AttributeNotFoundException"/>.
        /// </summary>
        /// <param name="member">Miembro sobre el que se ha buscado el atributo personalizado.</param>
        /// <param name="attributeType">Tipo del atributo personalizado que se ha buscado.</param>
        public AttributeNotFoundException(MemberInfo member, Type attributeType)
            : base(
                (member.DeclaringType == null
                    ? "El tipo '" + member.Name + "' "
                    : "El miembro '" + member.Name + "' del tipo '" + member.DeclaringType.Name + "' ")
                    + "no esta adornado con el atributo '" + attributeType.Name + "'."



                //            "El miembro '" + member.Name + "' del tipo '" + member.DeclaringType.Name +
                //					"' no esta adornado con el atributo '" + attributeType.Name + "'."
                )
        {
            Member = member;
            AttributeType = attributeType;
        }
    }
}
