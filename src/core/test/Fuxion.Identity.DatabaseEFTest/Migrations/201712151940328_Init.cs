namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        PackageDao_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PackageDao", t => t.PackageDao_Id)
                .Index(t => t.PackageDao_Id);
            
            CreateTable(
                "dbo.CategoryDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ParentId = c.String(maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryDao", t => t.ParentId)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.RolDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CategoryDao", t => t.CategoryId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.PermissionDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Function = c.String(),
                        Value = c.Boolean(nullable: false),
                        Rol_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RolDao", t => t.Rol_Id)
                .Index(t => t.Rol_Id);
            
            CreateTable(
                "dbo.ScopeDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Propagation = c.Int(nullable: false),
                        Name = c.String(),
                        PermissionDao_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PermissionDao", t => t.PermissionDao_Id)
                .Index(t => t.PermissionDao_Id);
            
            CreateTable(
                "dbo.SongDaoAlbumDaos",
                c => new
                    {
                        SongDao_Id = c.String(nullable: false, maxLength: 128),
                        AlbumDao_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.SongDao_Id, t.AlbumDao_Id })
                .ForeignKey("dbo.SongDao", t => t.SongDao_Id)
                .ForeignKey("dbo.AlbumDao", t => t.AlbumDao_Id)
                .Index(t => t.SongDao_Id)
                .Index(t => t.AlbumDao_Id);
            
            CreateTable(
                "dbo.RolDaoGroupDaos",
                c => new
                    {
                        RolDao_Id = c.String(nullable: false, maxLength: 128),
                        GroupDao_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.RolDao_Id, t.GroupDao_Id })
                .ForeignKey("dbo.RolDao", t => t.RolDao_Id)
                .ForeignKey("dbo.GroupDao", t => t.GroupDao_Id)
                .Index(t => t.RolDao_Id)
                .Index(t => t.GroupDao_Id);
            
            CreateTable(
                "dbo.PackageDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.AlbumDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PackageDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.DocumentDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.GroupDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RolDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.IdentityDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.Binary(),
                        PasswordSalt = c.Binary(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RolDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.MediaDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.SongDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MediaDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ExcelDocumentDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.PdfDocumentDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.WordDocumentDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentDao", t => t.Id)
                .ForeignKey("dbo.CategoryDao", t => t.CategoryId)
                .Index(t => t.Id)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.FilmDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MediaDao", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.SoftwareDao",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PackageDao", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SoftwareDao", "Id", "dbo.PackageDao");
            DropForeignKey("dbo.FilmDao", "Id", "dbo.MediaDao");
            DropForeignKey("dbo.WordDocumentDao", "CategoryId", "dbo.CategoryDao");
            DropForeignKey("dbo.WordDocumentDao", "Id", "dbo.DocumentDao");
            DropForeignKey("dbo.PdfDocumentDao", "Id", "dbo.DocumentDao");
            DropForeignKey("dbo.ExcelDocumentDao", "Id", "dbo.DocumentDao");
            DropForeignKey("dbo.SongDao", "Id", "dbo.MediaDao");
            DropForeignKey("dbo.MediaDao", "Id", "dbo.FileDao");
            DropForeignKey("dbo.IdentityDao", "Id", "dbo.RolDao");
            DropForeignKey("dbo.GroupDao", "Id", "dbo.RolDao");
            DropForeignKey("dbo.DocumentDao", "Id", "dbo.FileDao");
            DropForeignKey("dbo.AlbumDao", "Id", "dbo.PackageDao");
            DropForeignKey("dbo.PackageDao", "Id", "dbo.FileDao");
            DropForeignKey("dbo.ScopeDao", "PermissionDao_Id", "dbo.PermissionDao");
            DropForeignKey("dbo.PermissionDao", "Rol_Id", "dbo.RolDao");
            DropForeignKey("dbo.RolDaoGroupDaos", "GroupDao_Id", "dbo.GroupDao");
            DropForeignKey("dbo.RolDaoGroupDaos", "RolDao_Id", "dbo.RolDao");
            DropForeignKey("dbo.RolDao", "CategoryId", "dbo.CategoryDao");
            DropForeignKey("dbo.FileDao", "PackageDao_Id", "dbo.PackageDao");
            DropForeignKey("dbo.SongDaoAlbumDaos", "AlbumDao_Id", "dbo.AlbumDao");
            DropForeignKey("dbo.SongDaoAlbumDaos", "SongDao_Id", "dbo.SongDao");
            DropForeignKey("dbo.CategoryDao", "ParentId", "dbo.CategoryDao");
            DropIndex("dbo.SoftwareDao", new[] { "Id" });
            DropIndex("dbo.FilmDao", new[] { "Id" });
            DropIndex("dbo.WordDocumentDao", new[] { "CategoryId" });
            DropIndex("dbo.WordDocumentDao", new[] { "Id" });
            DropIndex("dbo.PdfDocumentDao", new[] { "Id" });
            DropIndex("dbo.ExcelDocumentDao", new[] { "Id" });
            DropIndex("dbo.SongDao", new[] { "Id" });
            DropIndex("dbo.MediaDao", new[] { "Id" });
            DropIndex("dbo.IdentityDao", new[] { "Id" });
            DropIndex("dbo.GroupDao", new[] { "Id" });
            DropIndex("dbo.DocumentDao", new[] { "Id" });
            DropIndex("dbo.AlbumDao", new[] { "Id" });
            DropIndex("dbo.PackageDao", new[] { "Id" });
            DropIndex("dbo.RolDaoGroupDaos", new[] { "GroupDao_Id" });
            DropIndex("dbo.RolDaoGroupDaos", new[] { "RolDao_Id" });
            DropIndex("dbo.SongDaoAlbumDaos", new[] { "AlbumDao_Id" });
            DropIndex("dbo.SongDaoAlbumDaos", new[] { "SongDao_Id" });
            DropIndex("dbo.ScopeDao", new[] { "PermissionDao_Id" });
            DropIndex("dbo.PermissionDao", new[] { "Rol_Id" });
            DropIndex("dbo.RolDao", new[] { "CategoryId" });
            DropIndex("dbo.CategoryDao", new[] { "ParentId" });
            DropIndex("dbo.FileDao", new[] { "PackageDao_Id" });
            DropTable("dbo.SoftwareDao");
            DropTable("dbo.FilmDao");
            DropTable("dbo.WordDocumentDao");
            DropTable("dbo.PdfDocumentDao");
            DropTable("dbo.ExcelDocumentDao");
            DropTable("dbo.SongDao");
            DropTable("dbo.MediaDao");
            DropTable("dbo.IdentityDao");
            DropTable("dbo.GroupDao");
            DropTable("dbo.DocumentDao");
            DropTable("dbo.AlbumDao");
            DropTable("dbo.PackageDao");
            DropTable("dbo.RolDaoGroupDaos");
            DropTable("dbo.SongDaoAlbumDaos");
            DropTable("dbo.ScopeDao");
            DropTable("dbo.PermissionDao");
            DropTable("dbo.RolDao");
            DropTable("dbo.CategoryDao");
            DropTable("dbo.FileDao");
        }
    }
}
