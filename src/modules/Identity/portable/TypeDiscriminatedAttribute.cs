using System;
namespace Fuxion.Identity
{
    /// <summary>
    /// Mark a class to define how will be discrimined by its type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TypeDiscriminatedAttribute :Attribute
    {
        public TypeDiscriminatedAttribute(string id) { Id = id; Name = id; }
        //public TypeDiscriminatedAttribute(bool enabled) { Enabled = enabled; }
        public TypeDiscriminatedAttribute(TypeDiscriminationDisableMode disableMode) { DisableMode = disableMode; }
        public string Id { get; set; }
        public string Name { get; set; }
        //public bool Enabled { get; set; } = true;
        public TypeDiscriminationDisableMode? DisableMode { get; private set; }

        public string[] ExplicitInclusions { get; set; }
        public string[] ExplicitExclusions { get; set; }
        public string[] AdditionalInclusions { get; set; }
        public string[] AdditionalExclusions { get; set; }
        public string[] AvoidedInclusions { get; set; }
        public string[] AvoidedExclusions { get; set; }
    }
    public enum TypeDiscriminationDisableMode
    {
        DisableType,
        DisableHierarchy
    }
}
