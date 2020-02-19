namespace BlogSystem.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleAddIsTop : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "IsTop", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Articles", "IsTop");
        }
    }
}
