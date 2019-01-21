using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Fuxion.Json
{
	public class ExceptionContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			property.ShouldSerialize = instance =>
			{
				try
				{
					var prop = (PropertyInfo)member;
					if (prop.CanRead)
					{
						prop.GetValue(instance, null);
						return true;
					}
				}
				catch { }
				return false;
			};
			if (property.PropertyType == typeof(string) && member.Name.Contains("StackTrace"))
			{
				property.ValueProvider = new GenericValueProvider<string, string[]>(
					property.ValueProvider,
					(PropertyInfo)member,
					str => str.SplitInLines(removeEmptyLines: true, trimEachLine: true));
				property.PropertyType = typeof(string[]);
			}
			if (typeof(MemberInfo).IsAssignableFrom(property.PropertyType))
			{
				property.ValueProvider = new GenericValueProvider<MemberInfo, string>(
					property.ValueProvider,
					(PropertyInfo)member,
					mi =>
					{
						if (mi is MethodBase mb) return $"{mi.GetType().Name} => {mb.GetSignature(true, true, true, true, true, true)}";
						return mi.Name;
					});
				property.PropertyType = typeof(string);
			}
			return property;
		}
	}
}
