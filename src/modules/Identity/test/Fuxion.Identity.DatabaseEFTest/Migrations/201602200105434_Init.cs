namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Rol",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Permission",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Value = c.Boolean(nullable: false),
                        Rol_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Rol", t => t.Rol_Id)
                .Index(t => t.Rol_Id);
            
            CreateTable(
                "dbo.Scope",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Propagation = c.Int(nullable: false),
                        Discriminator_Id = c.String(maxLength: 128),
                        Permission_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Discriminator_Id)
                .ForeignKey("dbo.Permission", t => t.Permission_Id)
                .Index(t => t.Discriminator_Id)
                .Index(t => t.Permission_Id);
            
            CreateTable(
                "dbo.Discriminator",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RolGroups",
                c => new
                    {
                        Rol_Id = c.String(nullable: false, maxLength: 128),
                        Group_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Rol_Id, t.Group_Id })
                .ForeignKey("dbo.Rol", t => t.Rol_Id)
                .ForeignKey("dbo.Group", t => t.Group_Id)
                .Index(t => t.Rol_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.DiscriminatorDiscriminators",
                c => new
                    {
                        Discriminator_Id = c.String(nullable: false, maxLength: 128),
                        Discriminator_Id1 = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Discriminator_Id, t.Discriminator_Id1 })
                .ForeignKey("dbo.Discriminator", t => t.Discriminator_Id)
                .ForeignKey("dbo.Discriminator", t => t.Discriminator_Id1)
                .Index(t => t.Discriminator_Id)
                .Index(t => t.Discriminator_Id1);
            
            CreateTable(
                "dbo.Identity",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.Binary(),
                        PasswordSalt = c.Binary(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Rol", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Group",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Rol", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Department",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Parent_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .ForeignKey("dbo.Department", t => t.Parent_Id)
                .Index(t => t.Id)
                .Index(t => t.Parent_Id);
            
            CreateTable(
                "dbo.City",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        State_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .ForeignKey("dbo.State", t => t.State_Id)
                .Index(t => t.Id)
                .Index(t => t.State_Id);
            
            CreateTable(
                "dbo.State",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Country_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .ForeignKey("dbo.Country", t => t.Country_Id)
                .Index(t => t.Id)
                .Index(t => t.Country_Id);
            
            CreateTable(
                "dbo.Country",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Tag",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tag", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Country", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.State", "Country_Id", "dbo.Country");
            DropForeignKey("dbo.State", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.City", "State_Id", "dbo.State");
            DropForeignKey("dbo.City", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Department", "Parent_Id", "dbo.Department");
            DropForeignKey("dbo.Department", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Category", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Group", "Id", "dbo.Rol");
            DropForeignKey("dbo.Identity", "Id", "dbo.Rol");
            DropForeignKey("dbo.Scope", "Permission_Id", "dbo.Permission");
            DropForeignKey("dbo.Scope", "Discriminator_Id", "dbo.Discriminator");
            DropForeignKey("dbo.DiscriminatorDiscriminators", "Discriminator_Id1", "dbo.Discriminator");
            DropForeignKey("dbo.DiscriminatorDiscriminators", "Discriminator_Id", "dbo.Discriminator");
            DropForeignKey("dbo.Permission", "Rol_Id", "dbo.Rol");
            DropForeignKey("dbo.RolGroups", "Group_Id", "dbo.Group");
            DropForeignKey("dbo.RolGroups", "Rol_Id", "dbo.Rol");
            DropIndex("dbo.Tag", new[] { "Id" });
            DropIndex("dbo.Country", new[] { "Id" });
            DropIndex("dbo.State", new[] { "Country_Id" });
            DropIndex("dbo.State", new[] { "Id" });
            DropIndex("dbo.City", new[] { "State_Id" });
            DropIndex("dbo.City", new[] { "Id" });
            DropIndex("dbo.Department", new[] { "Parent_Id" });
            DropIndex("dbo.Department", new[] { "Id" });
            DropIndex("dbo.Category", new[] { "Id" });
            DropIndex("dbo.Group", new[] { "Id" });
            DropIndex("dbo.Identity", new[] { "Id" });
            DropIndex("dbo.DiscriminatorDiscriminators", new[] { "Discriminator_Id1" });
            DropIndex("dbo.DiscriminatorDiscriminators", new[] { "Discriminator_Id" });
            DropIndex("dbo.RolGroups", new[] { "Group_Id" });
            DropIndex("dbo.RolGroups", new[] { "Rol_Id" });
            DropIndex("dbo.Scope", new[] { "Permission_Id" });
            DropIndex("dbo.Scope", new[] { "Discriminator_Id" });
            DropIndex("dbo.Permission", new[] { "Rol_Id" });
            DropTable("dbo.Tag");
            DropTable("dbo.Country");
            DropTable("dbo.State");
            DropTable("dbo.City");
            DropTable("dbo.Department");
            DropTable("dbo.Category");
            DropTable("dbo.Group");
            DropTable("dbo.Identity");
            DropTable("dbo.DiscriminatorDiscriminators");
            DropTable("dbo.RolGroups");
            DropTable("dbo.Discriminator");
            DropTable("dbo.Scope");
            DropTable("dbo.Permission");
            DropTable("dbo.Rol");
        }
    }
}
