namespace Fuxion.EntityFramework;

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Reflection;

public class CascadeDeleteConvention : IConceptualModelConvention<AssociationType>
{
	private static readonly Func<AssociationType, bool> IsSelfReferencing;
	private static readonly Func<AssociationType, bool> IsRequiredToMany;
	private static readonly Func<AssociationType, bool> IsManyToRequired;
	private static readonly Func<AssociationType, object?> GetConfiguration;
	private static readonly Func<object, OperationAction?> NavigationPropertyConfigurationGetDeleteAction;

	static CascadeDeleteConvention()
	{
		var associationTypeExtensionsType = typeof(AssociationType).Assembly.GetType("System.Data.Entity.ModelConfiguration.Edm.AssociationTypeExtensions");
		if (associationTypeExtensionsType == null)
			throw new InvalidProgramException("Assembly 'System.Data.Entity.ModelConfiguration.Edm.AssociationTypeExtensions' cannot be loaded");
		var navigationPropertyConfigurationType = typeof(AssociationType).Assembly.GetType("System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation.NavigationPropertyConfiguration");
		if (navigationPropertyConfigurationType == null)
			throw new InvalidProgramException("Assembly 'System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation.NavigationPropertyConfiguration' cannot be loaded");

		var isSelfRefencingMethod = associationTypeExtensionsType.GetMethod("IsSelfReferencing", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		if (isSelfRefencingMethod == null) throw new InvalidProgramException("Method 'IsSelfReferencing' cannot be found");
		IsSelfReferencing = associationType => (bool)(isSelfRefencingMethod.Invoke(null, new object[] { associationType }) ?? false);

		var isRequiredToManyMethod = associationTypeExtensionsType.GetMethod("IsRequiredToMany", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		if (isRequiredToManyMethod == null) throw new InvalidProgramException("Method 'IsRequiredToMany' cannot be found");
		IsRequiredToMany = associationType => (bool)(isRequiredToManyMethod.Invoke(null, new object[] { associationType }) ?? false);

		var isManyToRequiredMethod = associationTypeExtensionsType.GetMethod("IsManyToRequired", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		if (isManyToRequiredMethod == null) throw new InvalidProgramException("Method 'IsManyToRequired' cannot be found");
		IsManyToRequired = associationType => (bool)(isManyToRequiredMethod.Invoke(null, new object[] { associationType }) ?? false);

		var getConfigurationMethod = associationTypeExtensionsType.GetMethod("GetConfiguration", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		if (getConfigurationMethod == null) throw new InvalidProgramException("Method 'GetConfiguration' cannot be found");
		GetConfiguration = associationType => getConfigurationMethod.Invoke(null, new object[] { associationType });

		var deleteActionProperty = navigationPropertyConfigurationType.GetProperty("DeleteAction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		if (deleteActionProperty == null) throw new InvalidProgramException("Property 'DeleteAction' cannot be found");
		NavigationPropertyConfigurationGetDeleteAction = navProperty => (OperationAction?)deleteActionProperty.GetValue(navProperty);
	}

	public virtual void Apply(AssociationType item, DbModel model)
	{
		if (IsSelfReferencing(item))
			return;
		var propertyConfiguration = GetConfiguration(item);
		if (propertyConfiguration != null && NavigationPropertyConfigurationGetDeleteAction(propertyConfiguration).HasValue)
			return;
		AssociationEndMember? collectionEndMember = null;
		AssociationEndMember? singleNavigationEndMember = null;
		if (IsRequiredToMany(item))
		{
			collectionEndMember = GetSourceEnd(item);
			singleNavigationEndMember = GetTargetEnd(item);
		}
		else if (IsManyToRequired(item))
		{
			collectionEndMember = GetTargetEnd(item);
			singleNavigationEndMember = GetSourceEnd(item);
		}
		if (collectionEndMember == null || singleNavigationEndMember == null)
			return;

		var collectionCascadeDeleteAttribute = GetCascadeDeleteAttribute(collectionEndMember);
		var singleCascadeDeleteAttribute = GetCascadeDeleteAttribute(singleNavigationEndMember);

		if (collectionCascadeDeleteAttribute != null || singleCascadeDeleteAttribute != null)
			collectionEndMember.DeleteBehavior = OperationAction.Cascade;
	}

	private static AssociationEndMember? GetSourceEnd(AssociationType item) => item.KeyMembers.FirstOrDefault() as AssociationEndMember;
	private static AssociationEndMember? GetTargetEnd(AssociationType item) => item.KeyMembers.ElementAtOrDefault(1) as AssociationEndMember;

	private static CascadeDeleteAttribute? GetCascadeDeleteAttribute(EdmMember edmMember)
	{
		var clrProperties = edmMember.MetadataProperties.FirstOrDefault(m => m.Name == "ClrPropertyInfo");
		if (clrProperties == null)
			return null;

		var property = clrProperties.Value as PropertyInfo;
		if (property == null)
			return null;

		return property.GetCustomAttribute<CascadeDeleteAttribute>();
	}
}