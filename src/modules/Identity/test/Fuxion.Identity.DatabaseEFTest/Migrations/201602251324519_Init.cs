namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.File",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        FilePackage_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FilePackage", t => t.FilePackage_Id)
                .Index(t => t.FilePackage_Id);
            
            CreateTable(
                "dbo.Discriminator",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Demoes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ShipmentCityId = c.String(maxLength: 128),
                        ReceptionCityId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.City", t => t.ReceptionCityId)
                .ForeignKey("dbo.City", t => t.ShipmentCityId)
                .Index(t => t.ShipmentCityId)
                .Index(t => t.ReceptionCityId);
            
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
                        Function = c.String(),
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
                "dbo.Invoice",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DepartmentId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Department", t => t.DepartmentId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Order",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DepartmentId = c.String(maxLength: 128),
                        ShipmentCityId = c.String(maxLength: 128),
                        ReceptionCityId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Department", t => t.DepartmentId)
                .ForeignKey("dbo.City", t => t.ReceptionCityId)
                .ForeignKey("dbo.City", t => t.ShipmentCityId)
                .Index(t => t.DepartmentId)
                .Index(t => t.ShipmentCityId)
                .Index(t => t.ReceptionCityId);
            
            CreateTable(
                "dbo.CircleCircles",
                c => new
                    {
                        Circle_Id = c.String(nullable: false, maxLength: 128),
                        Circle_Id1 = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Circle_Id, t.Circle_Id1 })
                .ForeignKey("dbo.Circle", t => t.Circle_Id)
                .ForeignKey("dbo.Circle", t => t.Circle_Id1)
                .Index(t => t.Circle_Id)
                .Index(t => t.Circle_Id1);
            
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
                "dbo.FilePackage",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.File", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Album",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FilePackage", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Circle",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .Index(t => t.Id);
            
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
                "dbo.Song",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Album_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.File", t => t.Id)
                .ForeignKey("dbo.Album", t => t.Album_Id)
                .Index(t => t.Id)
                .Index(t => t.Album_Id);
            
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
                "dbo.Tag",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Seller",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Identity", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.PurchaseOrder",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Order", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.SellOrder",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Order", t => t.Id)
                .ForeignKey("dbo.Seller", t => t.SellerId)
                .Index(t => t.Id)
                .Index(t => t.SellerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SellOrder", "SellerId", "dbo.Seller");
            DropForeignKey("dbo.SellOrder", "Id", "dbo.Order");
            DropForeignKey("dbo.PurchaseOrder", "Id", "dbo.Order");
            DropForeignKey("dbo.Seller", "Id", "dbo.Identity");
            DropForeignKey("dbo.Tag", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Department", "Parent_Id", "dbo.Department");
            DropForeignKey("dbo.Department", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Category", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Group", "Id", "dbo.Rol");
            DropForeignKey("dbo.Country", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.State", "Country_Id", "dbo.Country");
            DropForeignKey("dbo.State", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.City", "State_Id", "dbo.State");
            DropForeignKey("dbo.City", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Song", "Album_Id", "dbo.Album");
            DropForeignKey("dbo.Song", "Id", "dbo.File");
            DropForeignKey("dbo.Identity", "Id", "dbo.Rol");
            DropForeignKey("dbo.Circle", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Album", "Id", "dbo.FilePackage");
            DropForeignKey("dbo.FilePackage", "Id", "dbo.File");
            DropForeignKey("dbo.Order", "ShipmentCityId", "dbo.City");
            DropForeignKey("dbo.Order", "ReceptionCityId", "dbo.City");
            DropForeignKey("dbo.Order", "DepartmentId", "dbo.Department");
            DropForeignKey("dbo.Invoice", "DepartmentId", "dbo.Department");
            DropForeignKey("dbo.Scope", "Permission_Id", "dbo.Permission");
            DropForeignKey("dbo.Scope", "Discriminator_Id", "dbo.Discriminator");
            DropForeignKey("dbo.Permission", "Rol_Id", "dbo.Rol");
            DropForeignKey("dbo.RolGroups", "Group_Id", "dbo.Group");
            DropForeignKey("dbo.RolGroups", "Rol_Id", "dbo.Rol");
            DropForeignKey("dbo.Demoes", "ShipmentCityId", "dbo.City");
            DropForeignKey("dbo.Demoes", "ReceptionCityId", "dbo.City");
            DropForeignKey("dbo.CircleCircles", "Circle_Id1", "dbo.Circle");
            DropForeignKey("dbo.CircleCircles", "Circle_Id", "dbo.Circle");
            DropForeignKey("dbo.File", "FilePackage_Id", "dbo.FilePackage");
            DropIndex("dbo.SellOrder", new[] { "SellerId" });
            DropIndex("dbo.SellOrder", new[] { "Id" });
            DropIndex("dbo.PurchaseOrder", new[] { "Id" });
            DropIndex("dbo.Seller", new[] { "Id" });
            DropIndex("dbo.Tag", new[] { "Id" });
            DropIndex("dbo.Department", new[] { "Parent_Id" });
            DropIndex("dbo.Department", new[] { "Id" });
            DropIndex("dbo.Category", new[] { "Id" });
            DropIndex("dbo.Group", new[] { "Id" });
            DropIndex("dbo.Country", new[] { "Id" });
            DropIndex("dbo.State", new[] { "Country_Id" });
            DropIndex("dbo.State", new[] { "Id" });
            DropIndex("dbo.City", new[] { "State_Id" });
            DropIndex("dbo.City", new[] { "Id" });
            DropIndex("dbo.Song", new[] { "Album_Id" });
            DropIndex("dbo.Song", new[] { "Id" });
            DropIndex("dbo.Identity", new[] { "Id" });
            DropIndex("dbo.Circle", new[] { "Id" });
            DropIndex("dbo.Album", new[] { "Id" });
            DropIndex("dbo.FilePackage", new[] { "Id" });
            DropIndex("dbo.RolGroups", new[] { "Group_Id" });
            DropIndex("dbo.RolGroups", new[] { "Rol_Id" });
            DropIndex("dbo.CircleCircles", new[] { "Circle_Id1" });
            DropIndex("dbo.CircleCircles", new[] { "Circle_Id" });
            DropIndex("dbo.Order", new[] { "ReceptionCityId" });
            DropIndex("dbo.Order", new[] { "ShipmentCityId" });
            DropIndex("dbo.Order", new[] { "DepartmentId" });
            DropIndex("dbo.Invoice", new[] { "DepartmentId" });
            DropIndex("dbo.Scope", new[] { "Permission_Id" });
            DropIndex("dbo.Scope", new[] { "Discriminator_Id" });
            DropIndex("dbo.Permission", new[] { "Rol_Id" });
            DropIndex("dbo.Demoes", new[] { "ReceptionCityId" });
            DropIndex("dbo.Demoes", new[] { "ShipmentCityId" });
            DropIndex("dbo.File", new[] { "FilePackage_Id" });
            DropTable("dbo.SellOrder");
            DropTable("dbo.PurchaseOrder");
            DropTable("dbo.Seller");
            DropTable("dbo.Tag");
            DropTable("dbo.Department");
            DropTable("dbo.Category");
            DropTable("dbo.Group");
            DropTable("dbo.Country");
            DropTable("dbo.State");
            DropTable("dbo.City");
            DropTable("dbo.Song");
            DropTable("dbo.Identity");
            DropTable("dbo.Circle");
            DropTable("dbo.Album");
            DropTable("dbo.FilePackage");
            DropTable("dbo.RolGroups");
            DropTable("dbo.CircleCircles");
            DropTable("dbo.Order");
            DropTable("dbo.Invoice");
            DropTable("dbo.Scope");
            DropTable("dbo.Permission");
            DropTable("dbo.Rol");
            DropTable("dbo.Demoes");
            DropTable("dbo.Discriminator");
            DropTable("dbo.File");
        }
    }
}
