using System.Reflection;

namespace Fuxion.Reflection;

/// <summary>
///    Excepción que se produce cuando se encuentra un determinado atributo personalizado un un miembro de un tipo más de una vez.
/// </summary>
public class AttributeMoreThanOneException : FuxionException
{
	/// <summary>
	///    Inicializa una nueva instancia de la clase <see cref="AttributeMoreThanOneException" />.
	/// </summary>
	/// <param name="member">Miembro sobre el que se ha buscado el atributo personalizado.</param>
	/// <param name="attributeType">Tipo del atributo personalizado que se ha buscado.</param>
	public AttributeMoreThanOneException(MemberInfo member, Type attributeType) : base("El miembro '" + member.Name + "' del tipo '" + member.DeclaringType?.Name + "' no esta adornado con el atributo '" + attributeType.Name + "'.")
	{
		Member = member;
		AttributeType = attributeType;
	}
	/// <summary>
	///    Obtiene el miembro para el cual se ha encontrado el atributo personalizado más de una vez.
	/// </summary>
	public MemberInfo Member { get; }
	/// <summary>
	///    Obtiene el tipo de atributo personalizado que ha sido encontrado más de una vez.
	/// </summary>
	public Type AttributeType { get; }
}