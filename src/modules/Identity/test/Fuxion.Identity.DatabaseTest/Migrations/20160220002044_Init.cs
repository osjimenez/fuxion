using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace Fuxion.Identity.DatabaseTest.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Discriminator",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discriminator", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Identity",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<byte[]>(nullable: true),
                    PasswordSalt = table.Column<byte[]>(nullable: true),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "RolGroup",
                columns: table => new
                {
                    RolId = table.Column<string>(nullable: false),
                    GroupId = table.Column<string>(nullable: false),
                    IdentityId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolGroup", x => new { x.RolId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_RolGroup_Identity_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolGroup_Group_RolId",
                        column: x => x.RolId,
                        principalTable: "Group",
                        principalColumn: "Id");
                });
            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IdentityId = table.Column<string>(nullable: true),
                    RolId = table.Column<string>(nullable: true),
                    Value = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permission_Identity_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Permission_Rol_RolId",
                        column: x => x.RolId,
                        principalTable: "Rol",
                        principalColumn: "Id");
                });
            migrationBuilder.CreateTable(
                name: "Scope",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DiscriminatorId = table.Column<string>(nullable: true),
                    PermissionId = table.Column<string>(nullable: true),
                    Propagation = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scope", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scope_Discriminator_DiscriminatorId",
                        column: x => x.DiscriminatorId,
                        principalTable: "Discriminator",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Scope_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("RolGroup");
            migrationBuilder.DropTable("Scope");
            migrationBuilder.DropTable("Group");
            migrationBuilder.DropTable("Discriminator");
            migrationBuilder.DropTable("Permission");
            migrationBuilder.DropTable("Identity");
            migrationBuilder.DropTable("Rol");
        }
    }
}
