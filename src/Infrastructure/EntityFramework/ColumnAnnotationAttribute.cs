using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.EntityFramework
{
    public abstract class ColumnAnnotationAttribute : Attribute
    {
        public static string? GetColumnName<TTrackAttribute>(EdmType type) where TTrackAttribute : ColumnAnnotationAttribute
        {
            var mems = type.MetadataProperties.Single(mp => mp.Name == "Members").Value as ReadOnlyMetadataCollection<EdmMember>;
            var pro = mems.SingleOrDefault(m => m.MetadataProperties.SingleOrDefault(p => p.Name.EndsWith("customannotation:" + (typeof(TTrackAttribute).FullName.Replace('.', '_')))) != null);
            return pro == null ? null : pro.Name;
        }
        public static void Annotate(Convention convention, IEnumerable<Type>? trackAttributes = null)
        {
            if (trackAttributes == null)
                trackAttributes = convention.GetType().Assembly.GetTypes().Where(t => typeof(ColumnAnnotationAttribute).IsAssignableFrom(t));
            foreach (var attType in trackAttributes)
            {
                convention.Properties()
                    .Having(p => p.CustomAttributes.SingleOrDefault(att => att.AttributeType.IsAssignableFrom(attType)))
                    .Configure((config, att) => config.HasColumnAnnotation(attType.FullName.Replace('.', '_'), ""));
            }
        }
    }
}