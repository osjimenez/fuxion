namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderTypes : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Order", name: "DomainId", newName: "DepartmentId");
            RenameIndex(table: "dbo.Order", name: "IX_DomainId", newName: "IX_DepartmentId");
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
            DropIndex("dbo.SellOrder", new[] { "SellerId" });
            DropIndex("dbo.SellOrder", new[] { "Id" });
            DropIndex("dbo.PurchaseOrder", new[] { "Id" });
            DropIndex("dbo.Seller", new[] { "Id" });
            DropTable("dbo.SellOrder");
            DropTable("dbo.PurchaseOrder");
            DropTable("dbo.Seller");
            RenameIndex(table: "dbo.Order", name: "IX_DepartmentId", newName: "IX_DomainId");
            RenameColumn(table: "dbo.Order", name: "DepartmentId", newName: "DomainId");
        }
    }
}
