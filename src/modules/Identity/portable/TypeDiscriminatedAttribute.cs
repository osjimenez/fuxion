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
        public TypeDiscriminatedAttribute(bool enabled) { Enabled = enabled; }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public string[] Inclusions { get; set; }
        public string[] Exclusions { get; set; }
    }
}
