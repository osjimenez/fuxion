namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Order : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Order",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DomainId = c.String(maxLength: 128),
                        ShipmentCityId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Department", t => t.DomainId)
                .ForeignKey("dbo.City", t => t.ShipmentCityId)
                .Index(t => t.DomainId)
                .Index(t => t.ShipmentCityId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Order", "ShipmentCityId", "dbo.City");
            DropForeignKey("dbo.Order", "DomainId", "dbo.Department");
            DropIndex("dbo.Order", new[] { "ShipmentCityId" });
            DropIndex("dbo.Order", new[] { "DomainId" });
            DropTable("dbo.Order");
        }
    }
}
