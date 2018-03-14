#if (NET471)
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fuxion.Data
{ 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TrackDisabledAttribute : ColumnAnnotationAttribute { }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackDeletedBitAttribute : ColumnAnnotationAttribute { }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackDeletedDateAttribute : ColumnAnnotationAttribute { }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackDeletedIdentityAttribute : ColumnAnnotationAttribute { }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackCreatedDateAttribute : ColumnAnnotationAttribute { }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackCreatedIdentityAttribute : ColumnAnnotationAttribute { }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackModifiedDateAttribute : ColumnAnnotationAttribute { }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TrackModifiedIdentityAttribute : ColumnAnnotationAttribute { }
    
    
    
}
#endif