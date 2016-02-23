namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSecondLocationDiscriminatorToOrders : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "ReceptionCityId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Order", "ReceptionCityId");
            AddForeignKey("dbo.Order", "ReceptionCityId", "dbo.City", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Order", "ReceptionCityId", "dbo.City");
            DropIndex("dbo.Order", new[] { "ReceptionCityId" });
            DropColumn("dbo.Order", "ReceptionCityId");
        }
    }
}
