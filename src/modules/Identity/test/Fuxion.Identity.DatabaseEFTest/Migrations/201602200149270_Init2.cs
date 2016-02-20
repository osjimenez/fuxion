namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Permission", "Function", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Permission", "Function");
        }
    }
}
