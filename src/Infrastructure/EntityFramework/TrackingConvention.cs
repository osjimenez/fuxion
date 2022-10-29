using System.Data.Entity.ModelConfiguration.Conventions;

namespace Fuxion.EntityFramework;

public class TrackingConvention : Convention
{
	public TrackingConvention(IEnumerable<Type>? trackAttributes = null) => ColumnAnnotationAttribute.Annotate(this, trackAttributes);
}