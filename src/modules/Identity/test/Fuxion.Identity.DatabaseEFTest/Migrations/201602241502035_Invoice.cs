namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Invoice : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Invoice", "DepartmentId", "dbo.Department");
            DropIndex("dbo.Invoice", new[] { "DepartmentId" });
            DropTable("dbo.Invoice");
        }
    }
}
