using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    [DebuggerDisplay(nameof(Name))]
    class GuidDiscriminator : IDiscriminator<Guid, Guid>
    {
        public GuidDiscriminator(Guid id, string name, IEnumerable<Guid> inclusions, IEnumerable<Guid> exclusions, Guid typeId, string typeName)
        {
            Id = id;
            Name = name;
            Inclusions = inclusions;
            Exclusions = exclusions;
            TypeId = typeId;
            TypeName = typeName;
        }
        public Guid Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }
        public string Name { get; private set; }
        IEnumerable<object> IDiscriminator.Inclusions { get { return ((IDiscriminator<Guid, Guid>)this).Inclusions.Cast<object>(); } }
        public IEnumerable<Guid> Inclusions { get; set; }
        IEnumerable<object> IDiscriminator.Exclusions { get { return ((IDiscriminator<Guid, Guid>)this).Exclusions.Cast<object>(); } }
        public IEnumerable<Guid> Exclusions { get; set; }
        public Guid TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get; private set; }
    }
    [DebuggerDisplay(nameof(Name))]
    class StringDiscriminator : IDiscriminator<string, string>
    {
        public StringDiscriminator(string id, IEnumerable<string> inclusions, IEnumerable<string> exclusions, string typeId)
        {
            Id = id;
            Inclusions = inclusions;
            Exclusions = exclusions;
            TypeId = typeId;
        }
        public string Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }
        public string Name { get { return Id; } }
        IEnumerable<object> IDiscriminator.Inclusions { get { return ((IDiscriminator<string, string>)this).Inclusions.Cast<object>(); } }
        public IEnumerable<string> Inclusions { get; set; }
        IEnumerable<object> IDiscriminator.Exclusions { get { return ((IDiscriminator<string, string>)this).Exclusions.Cast<object>(); } }
        public IEnumerable<string> Exclusions { get; set; }
        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get { return TypeId; } }
    }
    [DebuggerDisplay(nameof(Name))]
    class GuidStringDiscriminator : IDiscriminator<Guid, string>
    {
        public GuidStringDiscriminator(Guid id, string name, IEnumerable<Guid> inclusions, IEnumerable<Guid> exclusions, string typeId)
        {
            Id = id;
            Name = name;
            Inclusions = inclusions;
            Exclusions = exclusions;
            TypeId = typeId;
        }
        public Guid Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }
        public string Name { get; private set; }
        IEnumerable<object> IDiscriminator.Inclusions { get { return ((IDiscriminator<Guid, Guid>)this).Inclusions.Cast<object>(); } }
        public IEnumerable<Guid> Inclusions { get; set; }
        IEnumerable<object> IDiscriminator.Exclusions { get { return ((IDiscriminator<Guid, Guid>)this).Exclusions.Cast<object>(); } }
        public IEnumerable<Guid> Exclusions { get; set; }
        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get { return TypeId; } }
    }
    [TestClass]
    public class DiscriminatorTest
    {
        [TestMethod]
        public void Validate_GuidDiscriminator()
        {
            // Invalid by Id
            Assert.IsFalse(new GuidDiscriminator(default(Guid) , "valid", null, null, Guid.NewGuid(), "valid").IsValid());

            // Invalid by Name
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), null   , null, null, Guid.NewGuid(), "valid").IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), ""     , null, null, Guid.NewGuid(), "valid").IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), " "    , null, null, Guid.NewGuid(), "valid").IsValid());

            // Invalid by TypeId
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", null, null, default(Guid) , "valid").IsValid());

            // Invalid by TypeName
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", null, null, Guid.NewGuid(), null).IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", null, null, Guid.NewGuid(), "").IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", null, null, Guid.NewGuid(), " ").IsValid());

            // Valid
            Assert.IsTrue(new GuidDiscriminator(Guid.NewGuid() , "valid", null, null, Guid.NewGuid(), "valid").IsValid());
        }
        [TestMethod]
        public void Validate_StringDiscriminator()
        {
            // Name and Id are same value
            // Invalid by Name
            Assert.IsFalse(new StringDiscriminator(null, null, null, "valid").IsValid());
            Assert.IsFalse(new StringDiscriminator(""  , null, null, "valid").IsValid());
            Assert.IsFalse(new StringDiscriminator(" " , null, null, "valid").IsValid());

            // TypeName and TypeId are same value
            // Invalid by TypeName
            Assert.IsFalse(new StringDiscriminator("valid", null, null, null).IsValid());
            Assert.IsFalse(new StringDiscriminator("valid", null, null, ""  ).IsValid());
            Assert.IsFalse(new StringDiscriminator("valid", null, null, " ").IsValid());

            // Valid
            Assert.IsTrue(new StringDiscriminator("valid", null, null, "valid").IsValid());
        }
        [TestMethod]
        public void Validate_GuidStringDiscriminator()
        {
            // Invalid by Id
            Assert.IsFalse(new GuidStringDiscriminator(default(Guid), "valid", null, null, "valid").IsValid());

            // Invalid by Name
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), null, null, null, "valid").IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "", null, null, "valid").IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), " ", null, null, "valid").IsValid());

            // TypeName and TypeId are same value
            // Invalid by Typename
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, null).IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, "").IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, " ").IsValid());

            // Valid
            Assert.IsTrue(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, "valid").IsValid());
        }

        [TestMethod]
        public void Compare_GuidDiscriminator()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var comparer = new DiscriminatorEqualityComparer();
            Assert.IsFalse(comparer.Equals(
                new GuidDiscriminator(id1, null, null, null, id3, null),
                new GuidDiscriminator(id2, null, null, null, id3, null)),
                "The comparison should fail if identifications(Id) differ");
            Assert.IsFalse(comparer.Equals(
                new GuidDiscriminator(id1, null, null, null, id2, null),
                new GuidDiscriminator(id1, null, null, null, id3, null)),
                "The comparison should fail if type identifications(TypeId) differ");
            Assert.IsTrue(comparer.Equals(
                new GuidDiscriminator(id1, null, null, null, id3, null),
                new GuidDiscriminator(id1, null, null, null, id3, null)),
                "The comparison should pass if identifications(Id) and type identifications(TypeId) are same");
        }
        [TestMethod]
        public void Compare_StringDiscriminator()
        {
            var id1 = "id1";
            var id2 = "id2";
            var id3 = "id3";
            var comparer = new DiscriminatorEqualityComparer();
            Assert.IsFalse(comparer.Equals(
                new StringDiscriminator(id1, null, null, id3),
                new StringDiscriminator(id2, null, null, id3)),
                "The comparison should fail if identifications(Id) differ");
            Assert.IsFalse(comparer.Equals(
                new StringDiscriminator(id1, null, null, id2),
                new StringDiscriminator(id1, null, null, id3)),
                "The comparison should fail if type identifications(TypeId) differ");
            Assert.IsTrue(comparer.Equals(
                new StringDiscriminator(id1, null, null, id3),
                new StringDiscriminator(id1, null, null, id3)),
                "The comparison should pass if identifications(Id) and type identifications(TypeId) are same");
        }
    }
}
