namespace Fuxion.EntityFramework;

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