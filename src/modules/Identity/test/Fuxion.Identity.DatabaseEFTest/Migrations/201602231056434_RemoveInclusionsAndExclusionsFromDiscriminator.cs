namespace Fuxion.Identity.DatabaseEFTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveInclusionsAndExclusionsFromDiscriminator : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DiscriminatorDiscriminators", "Discriminator_Id", "dbo.Discriminator");
            DropForeignKey("dbo.DiscriminatorDiscriminators", "Discriminator_Id1", "dbo.Discriminator");
            DropIndex("dbo.DiscriminatorDiscriminators", new[] { "Discriminator_Id" });
            DropIndex("dbo.DiscriminatorDiscriminators", new[] { "Discriminator_Id1" });
            DropTable("dbo.DiscriminatorDiscriminators");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DiscriminatorDiscriminators",
                c => new
                    {
                        Discriminator_Id = c.String(nullable: false, maxLength: 128),
                        Discriminator_Id1 = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Discriminator_Id, t.Discriminator_Id1 });
            
            CreateIndex("dbo.DiscriminatorDiscriminators", "Discriminator_Id1");
            CreateIndex("dbo.DiscriminatorDiscriminators", "Discriminator_Id");
            AddForeignKey("dbo.DiscriminatorDiscriminators", "Discriminator_Id1", "dbo.Discriminator", "Id");
            AddForeignKey("dbo.DiscriminatorDiscriminators", "Discriminator_Id", "dbo.Discriminator", "Id");
        }
    }
}
