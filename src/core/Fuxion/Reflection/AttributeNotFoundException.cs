using System.Reflection;

namespace Fuxion.Reflection;

/// <summary>
///    Excepción que se produce cuando no puede encontrarse un determinado atributo personalizado un un miembro de un tipo.
/// </summary>
public class AttributeNotFoundException : FuxionException
{
	/// <summary>
	///    Inicializa una nueva instancia de la clase <see cref="AttributeNotFoundException" />.
	/// </summary>
	/// <param name="member">Miembro sobre el que se ha buscado el atributo personalizado.</param>
	/// <param name="attributeType">Tipo del atributo personalizado que se ha buscado.</param>
	public AttributeNotFoundException(MemberInfo member, Type attributeType) : base(
		(member.DeclaringType == null 
			? "Type '" + (member is Type t1 ? t1.GetSignature() : member.Name) + "' "
			: "The member '" + (member is Type t2 ? t2.GetSignature() : member.Name) + "' of the type '" 
				+ member.DeclaringType.GetSignature() + "' ") + "it's not adorned with attribute '"
				+ attributeType.GetSignature() + "'.")
	{
		Member = member;
		AttributeType = attributeType;
	}
	/// <summary>
	///    Obtiene el miembro para el cual no se ha encontrado el atributo personalizado.
	/// </summary>
	public MemberInfo Member { get; }
	/// <summary>
	///    Obtiene el tipo de atributo personalizado que no ha podido ser encontrado.
	/// </summary>
	public Type AttributeType { get; }
}