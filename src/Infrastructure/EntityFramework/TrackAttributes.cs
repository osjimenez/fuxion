namespace Fuxion.EntityFramework;

[AttributeUsage(AttributeTargets.Class)]
public class TrackDisabledAttribute : ColumnAnnotationAttribute { }

[AttributeUsage(AttributeTargets.Property)]
public class TrackDeletedBitAttribute : ColumnAnnotationAttribute { }

[AttributeUsage(AttributeTargets.Property)]
public class TrackDeletedDateAttribute : ColumnAnnotationAttribute { }

[AttributeUsage(AttributeTargets.Property)]
public class TrackDeletedIdentityAttribute : ColumnAnnotationAttribute { }

[AttributeUsage(AttributeTargets.Property)]
public class TrackCreatedDateAttribute : ColumnAnnotationAttribute { }

[AttributeUsage(AttributeTargets.Property)]
public class TrackCreatedIdentityAttribute : ColumnAnnotationAttribute { }

[AttributeUsage(AttributeTargets.Property)]
public class TrackModifiedDateAttribute : ColumnAnnotationAttribute { }

[AttributeUsage(AttributeTargets.Property)]
public class TrackModifiedIdentityAttribute : ColumnAnnotationAttribute { }