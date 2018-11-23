using System;
namespace Fuxion.Identity
{
	/// <summary>
	/// Mark a class to be a discriminator for other classes
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DiscriminatorAttribute : Attribute
	{
		public DiscriminatorAttribute(object typeId) => TypeId = typeId;
		public object TypeId { get; set; }
	}
}
