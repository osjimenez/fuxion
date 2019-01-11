using System;
using Fuxion.Identity.DatabaseTest;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Fuxion.Identity.DatabaseTest.Migrations
{
    [DbContext(typeof(IdentityDatabaseRepository))]
    partial class IdentityDatabaseRepositoryModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-beta8-15964")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Discriminator", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Discriminator");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Group", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Group");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Identity", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Name");

                    b.Property<byte[]>("PasswordHash");

                    b.Property<byte[]>("PasswordSalt");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Identity");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Permission", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("IdentityId");

                    b.Property<string>("RolId");

                    b.Property<bool>("Value");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Permission");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Rol", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Rol");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.RolGroup", b =>
                {
                    b.Property<string>("RolId");

                    b.Property<string>("GroupId");

                    b.Property<string>("IdentityId");

                    b.HasKey("RolId", "GroupId");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Scope", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("DiscriminatorId");

                    b.Property<string>("PermissionId");

                    b.Property<int>("Propagation");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "Scope");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Permission", b =>
                {
                    b.HasOne("Fuxion.Identity.Test.Entity.Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId");

                    b.HasOne("Fuxion.Identity.Test.Entity.Rol")
                        .WithMany()
                        .HasForeignKey("RolId");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.RolGroup", b =>
                {
                    b.HasOne("Fuxion.Identity.Test.Entity.Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId");

                    b.HasOne("Fuxion.Identity.Test.Entity.Group")
                        .WithMany()
                        .HasForeignKey("RolId");
                });

            modelBuilder.Entity("Fuxion.Identity.Test.Entity.Scope", b =>
                {
                    b.HasOne("Fuxion.Identity.Test.Entity.Discriminator")
                        .WithMany()
                        .HasForeignKey("DiscriminatorId");

                    b.HasOne("Fuxion.Identity.Test.Entity.Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId");
                });
        }
    }
}
