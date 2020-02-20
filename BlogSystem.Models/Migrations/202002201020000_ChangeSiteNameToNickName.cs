namespace BlogSystem.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeSiteNameToNickName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Nickname", c => c.String());
            DropColumn("dbo.Users", "SiteName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "SiteName", c => c.String());
            DropColumn("dbo.Users", "Nickname");
        }
    }
}
