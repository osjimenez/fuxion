namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class All : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Song", "Id", "dbo.File");
            CreateTable(
                "dbo.Media",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.File", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Document",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.File", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ExcelDocument",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Document", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.PdfDocument",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Document", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.WordDocument",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Document", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Software",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FilePackage", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Film",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Media", t => t.Id)
                .Index(t => t.Id);
            
            AddColumn("dbo.Circle", "Song_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Circle", "Album_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Circle", "Song_Id");
            CreateIndex("dbo.Circle", "Album_Id");
            AddForeignKey("dbo.Circle", "Song_Id", "dbo.Song", "Id");
            AddForeignKey("dbo.Circle", "Album_Id", "dbo.Album", "Id");
            AddForeignKey("dbo.Song", "Id", "dbo.Media", "Id");
            DropColumn("dbo.File", "Discriminator");
            DropColumn("dbo.FilePackage", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FilePackage", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.File", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.Film", "Id", "dbo.Media");
            DropForeignKey("dbo.Software", "Id", "dbo.FilePackage");
            DropForeignKey("dbo.WordDocument", "Id", "dbo.Document");
            DropForeignKey("dbo.PdfDocument", "Id", "dbo.Document");
            DropForeignKey("dbo.ExcelDocument", "Id", "dbo.Document");
            DropForeignKey("dbo.Document", "Id", "dbo.File");
            DropForeignKey("dbo.Song", "Id", "dbo.Media");
            DropForeignKey("dbo.Media", "Id", "dbo.File");
            DropForeignKey("dbo.Circle", "Album_Id", "dbo.Album");
            DropForeignKey("dbo.Circle", "Song_Id", "dbo.Song");
            DropIndex("dbo.Film", new[] { "Id" });
            DropIndex("dbo.Software", new[] { "Id" });
            DropIndex("dbo.WordDocument", new[] { "Id" });
            DropIndex("dbo.PdfDocument", new[] { "Id" });
            DropIndex("dbo.ExcelDocument", new[] { "Id" });
            DropIndex("dbo.Document", new[] { "Id" });
            DropIndex("dbo.Media", new[] { "Id" });
            DropIndex("dbo.Circle", new[] { "Album_Id" });
            DropIndex("dbo.Circle", new[] { "Song_Id" });
            DropColumn("dbo.Circle", "Album_Id");
            DropColumn("dbo.Circle", "Song_Id");
            DropTable("dbo.Film");
            DropTable("dbo.Software");
            DropTable("dbo.WordDocument");
            DropTable("dbo.PdfDocument");
            DropTable("dbo.ExcelDocument");
            DropTable("dbo.Document");
            DropTable("dbo.Media");
            AddForeignKey("dbo.Song", "Id", "dbo.File", "Id");
        }
    }
}
