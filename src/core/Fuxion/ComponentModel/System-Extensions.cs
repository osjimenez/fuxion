using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fuxion.ComponentModel;

public static class PropertyDescriptorExtensions
{
	public static string GetDisplayName(this PropertyDescriptor me)
	{
		var att = me.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
		return att?.GetName() ?? me.DisplayName;
	}
}