namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlbumsInSong : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Song", "Album_Id", "dbo.Album");
            DropIndex("dbo.Song", new[] { "Album_Id" });
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
            
            DropColumn("dbo.Song", "Album_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Song", "Album_Id", c => c.String(maxLength: 128));
            DropForeignKey("dbo.SongAlbums", "Album_Id", "dbo.Album");
            DropForeignKey("dbo.SongAlbums", "Song_Id", "dbo.Song");
            DropIndex("dbo.SongAlbums", new[] { "Album_Id" });
            DropIndex("dbo.SongAlbums", new[] { "Song_Id" });
            DropTable("dbo.SongAlbums");
            CreateIndex("dbo.Song", "Album_Id");
            AddForeignKey("dbo.Song", "Album_Id", "dbo.Album", "Id");
        }
    }
}
