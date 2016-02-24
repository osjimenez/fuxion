namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Demo : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Demoes", "ShipmentCityId", "dbo.City");
            DropForeignKey("dbo.Demoes", "ReceptionCityId", "dbo.City");
            DropIndex("dbo.Demoes", new[] { "ReceptionCityId" });
            DropIndex("dbo.Demoes", new[] { "ShipmentCityId" });
            DropTable("dbo.Demoes");
        }
    }
}
