namespace Fuxion.EntityFramework;

using System.Data.Entity.ModelConfiguration.Conventions;

public class TrackingConvention : Convention
{
	public TrackingConvention(IEnumerable<Type>? trackAttributes = null) => ColumnAnnotationAttribute.Annotate(this, trackAttributes);
}