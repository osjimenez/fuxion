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
                        FilePackage_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FilePackage", t => t.FilePackage_Id)
                .Index(t => t.FilePackage_Id);
            
            CreateTable(
                "dbo.Writer",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Document_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Document", t => t.Document_Id)
                .Index(t => t.Document_Id);
            
            CreateTable(
                "dbo.Actor",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Film_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Film", t => t.Film_Id)
                .Index(t => t.Film_Id);
            
            CreateTable(
                "dbo.FilmDirector",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Screenwriter",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Film_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Film", t => t.Film_Id)
                .Index(t => t.Film_Id);
            
            CreateTable(
                "dbo.Discriminator",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.SongAlbums",
                c => new
                    {
                        Song_Id = c.String(nullable: false, maxLength: 128),
                        Album_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Song_Id, t.Album_Id })
                .ForeignKey("dbo.Song", t => t.Song_Id)
                .ForeignKey("dbo.Album", t => t.Album_Id)
                .Index(t => t.Song_Id)
                .Index(t => t.Album_Id);
            
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
                        Song_Id = c.String(maxLength: 128),
                        Album_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Discriminator", t => t.Id)
                .ForeignKey("dbo.Song", t => t.Song_Id)
                .ForeignKey("dbo.Album", t => t.Album_Id)
                .Index(t => t.Id)
                .Index(t => t.Song_Id)
                .Index(t => t.Album_Id);
            
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
                "dbo.Media",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.File", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Song",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Media", t => t.Id)
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
                        Director_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Media", t => t.Id)
                .ForeignKey("dbo.FilmDirector", t => t.Director_Id)
                .Index(t => t.Id)
                .Index(t => t.Director_Id);
            
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
            DropForeignKey("dbo.Category", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Group", "Id", "dbo.Rol");
            DropForeignKey("dbo.Film", "Director_Id", "dbo.FilmDirector");
            DropForeignKey("dbo.Film", "Id", "dbo.Media");
            DropForeignKey("dbo.Software", "Id", "dbo.FilePackage");
            DropForeignKey("dbo.WordDocument", "Id", "dbo.Document");
            DropForeignKey("dbo.PdfDocument", "Id", "dbo.Document");
            DropForeignKey("dbo.ExcelDocument", "Id", "dbo.Document");
            DropForeignKey("dbo.Document", "Id", "dbo.File");
            DropForeignKey("dbo.Song", "Id", "dbo.Media");
            DropForeignKey("dbo.Media", "Id", "dbo.File");
            DropForeignKey("dbo.Identity", "Id", "dbo.Rol");
            DropForeignKey("dbo.Circle", "Album_Id", "dbo.Album");
            DropForeignKey("dbo.Circle", "Song_Id", "dbo.Song");
            DropForeignKey("dbo.Circle", "Id", "dbo.Discriminator");
            DropForeignKey("dbo.Album", "Id", "dbo.FilePackage");
            DropForeignKey("dbo.FilePackage", "Id", "dbo.File");
            DropForeignKey("dbo.Scope", "Permission_Id", "dbo.Permission");
            DropForeignKey("dbo.Scope", "Discriminator_Id", "dbo.Discriminator");
            DropForeignKey("dbo.Permission", "Rol_Id", "dbo.Rol");
            DropForeignKey("dbo.RolGroups", "Group_Id", "dbo.Group");
            DropForeignKey("dbo.RolGroups", "Rol_Id", "dbo.Rol");
            DropForeignKey("dbo.CircleCircles", "Circle_Id1", "dbo.Circle");
            DropForeignKey("dbo.CircleCircles", "Circle_Id", "dbo.Circle");
            DropForeignKey("dbo.SongAlbums", "Album_Id", "dbo.Album");
            DropForeignKey("dbo.SongAlbums", "Song_Id", "dbo.Song");
            DropForeignKey("dbo.Screenwriter", "Film_Id", "dbo.Film");
            DropForeignKey("dbo.Actor", "Film_Id", "dbo.Film");
            DropForeignKey("dbo.File", "FilePackage_Id", "dbo.FilePackage");
            DropForeignKey("dbo.Writer", "Document_Id", "dbo.Document");
            DropIndex("dbo.Tag", new[] { "Id" });
            DropIndex("dbo.Country", new[] { "Id" });
            DropIndex("dbo.State", new[] { "Country_Id" });
            DropIndex("dbo.State", new[] { "Id" });
            DropIndex("dbo.City", new[] { "State_Id" });
            DropIndex("dbo.City", new[] { "Id" });
            DropIndex("dbo.Category", new[] { "Id" });
            DropIndex("dbo.Group", new[] { "Id" });
            DropIndex("dbo.Film", new[] { "Director_Id" });
            DropIndex("dbo.Film", new[] { "Id" });
            DropIndex("dbo.Software", new[] { "Id" });
            DropIndex("dbo.WordDocument", new[] { "Id" });
            DropIndex("dbo.PdfDocument", new[] { "Id" });
            DropIndex("dbo.ExcelDocument", new[] { "Id" });
            DropIndex("dbo.Document", new[] { "Id" });
            DropIndex("dbo.Song", new[] { "Id" });
            DropIndex("dbo.Media", new[] { "Id" });
            DropIndex("dbo.Identity", new[] { "Id" });
            DropIndex("dbo.Circle", new[] { "Album_Id" });
            DropIndex("dbo.Circle", new[] { "Song_Id" });
            DropIndex("dbo.Circle", new[] { "Id" });
            DropIndex("dbo.Album", new[] { "Id" });
            DropIndex("dbo.FilePackage", new[] { "Id" });
            DropIndex("dbo.RolGroups", new[] { "Group_Id" });
            DropIndex("dbo.RolGroups", new[] { "Rol_Id" });
            DropIndex("dbo.CircleCircles", new[] { "Circle_Id1" });
            DropIndex("dbo.CircleCircles", new[] { "Circle_Id" });
            DropIndex("dbo.SongAlbums", new[] { "Album_Id" });
            DropIndex("dbo.SongAlbums", new[] { "Song_Id" });
            DropIndex("dbo.Scope", new[] { "Permission_Id" });
            DropIndex("dbo.Scope", new[] { "Discriminator_Id" });
            DropIndex("dbo.Permission", new[] { "Rol_Id" });
            DropIndex("dbo.Screenwriter", new[] { "Film_Id" });
            DropIndex("dbo.Actor", new[] { "Film_Id" });
            DropIndex("dbo.Writer", new[] { "Document_Id" });
            DropIndex("dbo.File", new[] { "FilePackage_Id" });
            DropTable("dbo.Tag");
            DropTable("dbo.Country");
            DropTable("dbo.State");
            DropTable("dbo.City");
            DropTable("dbo.Category");
            DropTable("dbo.Group");
            DropTable("dbo.Film");
            DropTable("dbo.Software");
            DropTable("dbo.WordDocument");
            DropTable("dbo.PdfDocument");
            DropTable("dbo.ExcelDocument");
            DropTable("dbo.Document");
            DropTable("dbo.Song");
            DropTable("dbo.Media");
            DropTable("dbo.Identity");
            DropTable("dbo.Circle");
            DropTable("dbo.Album");
            DropTable("dbo.FilePackage");
            DropTable("dbo.RolGroups");
            DropTable("dbo.CircleCircles");
            DropTable("dbo.SongAlbums");
            DropTable("dbo.Scope");
            DropTable("dbo.Permission");
            DropTable("dbo.Rol");
            DropTable("dbo.Discriminator");
            DropTable("dbo.Screenwriter");
            DropTable("dbo.FilmDirector");
            DropTable("dbo.Actor");
            DropTable("dbo.Writer");
            DropTable("dbo.File");
        }
    }
}
