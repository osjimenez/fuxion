using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Identity.Test.Mocks;
using Fuxion.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Fuxion.Identity.Functions;

namespace Fuxion.Identity.Test
{
    public class DiscriminatorTest : BaseTest
    {
        public DiscriminatorTest(ITestOutputHelper helper) : base(helper) { }
        [Fact(DisplayName = "Discriminator - Validate guid type")]
        public void Validate_GuidDiscriminator()
        {
            var ttt = new GuidDiscriminator(default(Guid),       "valid", Guid.NewGuid(), "valid");

            // Invalid by Id
            Assert.True(new GuidDiscriminator(default(Guid) , "valid", Guid.NewGuid(), "valid").IsValid());

            // Invalid by Name
            Assert.True(new GuidDiscriminator(Guid.NewGuid(), null   , Guid.NewGuid(), "valid").IsValid());
            Assert.True(new GuidDiscriminator(Guid.NewGuid(), ""     , Guid.NewGuid(), "valid").IsValid());
            Assert.True(new GuidDiscriminator(Guid.NewGuid(), " "    , Guid.NewGuid(), "valid").IsValid());

            // Invalid by TypeId
            Assert.False(new GuidDiscriminator(Guid.NewGuid(), "valid", default(Guid) , "valid").IsValid());

            // Invalid by TypeName
            Assert.False(new GuidDiscriminator(Guid.NewGuid(), "valid", Guid.NewGuid(), null).IsValid());
            Assert.False(new GuidDiscriminator(Guid.NewGuid(), "valid", Guid.NewGuid(), "").IsValid());
            Assert.False(new GuidDiscriminator(Guid.NewGuid(), "valid", Guid.NewGuid(), " ").IsValid());

            // Valid
            Assert.True(new GuidDiscriminator(Guid.NewGuid() , "valid", Guid.NewGuid(), "valid").IsValid());
        }
        [Fact(DisplayName = "Discriminator - Validate string type")]
        public void Validate_StringDiscriminator()
        {
            // Name and Id are same value
            Assert.True(new StringDiscriminator(null, null, null, "valid").IsValid());
            Assert.True(new StringDiscriminator(""  , null, null, "valid").IsValid());
            Assert.True(new StringDiscriminator(" " , null, null, "valid").IsValid());

            // TypeName and TypeId are same value
            Assert.False(new StringDiscriminator("valid", null, null, null).IsValid());
            Assert.False(new StringDiscriminator("valid", null, null, ""  ).IsValid());
            Assert.False(new StringDiscriminator("valid", null, null, " ").IsValid());

            // Valid
            Assert.True(new StringDiscriminator("valid", null, null, "valid").IsValid());
        }
        [Fact(DisplayName = "Discriminator - Validate guid string type")]
        public void Validate_GuidStringDiscriminator()
        {
            // Id
            Assert.True(new GuidStringDiscriminator(default(Guid), "valid", null, null, "valid").IsValid());

            // Name
            Assert.True(new GuidStringDiscriminator(Guid.NewGuid(), null, null, null, "valid").IsValid());
            Assert.True(new GuidStringDiscriminator(Guid.NewGuid(), "", null, null, "valid").IsValid());
            Assert.True(new GuidStringDiscriminator(Guid.NewGuid(), " ", null, null, "valid").IsValid());

            // TypeName and TypeId are same value
            Assert.False(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, null).IsValid());
            Assert.False(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, "").IsValid());
            Assert.False(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, " ").IsValid());

            // All valid
            Assert.True(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, "valid").IsValid());
        }

        [Fact(DisplayName = "Discriminator - Compare guid type")]
        public void Compare_GuidDiscriminator()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var comparer = new DiscriminatorEqualityComparer();
            Assert.False(comparer.Equals(
                new GuidDiscriminator(id1, null, id3, null),
                new GuidDiscriminator(id2, null, id3, null)),
                "The comparison should fail if identifications(Id) differ");
            Assert.False(comparer.Equals(
                new GuidDiscriminator(id1, null, id2, null),
                new GuidDiscriminator(id1, null, id3, null)),
                "The comparison should fail if type identifications(TypeId) differ");
            Assert.True(comparer.Equals(
                new GuidDiscriminator(id1, null, id3, null),
                new GuidDiscriminator(id1, null, id3, null)),
                "The comparison should pass if identifications(Id) and type identifications(TypeId) are same");
        }
        [Fact(DisplayName = "Discriminator - Compare string type")]
        public void Compare_StringDiscriminator()
        {
            var id1 = "id1";
            var id2 = "id2";
            var id3 = "id3";
            var comparer = new DiscriminatorEqualityComparer();
            Assert.False(comparer.Equals(
                new StringDiscriminator(id1, null, null, id3),
                new StringDiscriminator(id2, null, null, id3)),
                "The comparison should fail if identifications(Id) differ");
            Assert.False(comparer.Equals(
                new StringDiscriminator(id1, null, null, id2),
                new StringDiscriminator(id1, null, null, id3)),
                "The comparison should fail if type identifications(TypeId) differ");
            Assert.True(comparer.Equals(
                new StringDiscriminator(id1, null, null, id3),
                new StringDiscriminator(id1, null, null, id3)),
                "The comparison should pass if identifications(Id) and type identifications(TypeId) are same");
        }
    }
}
