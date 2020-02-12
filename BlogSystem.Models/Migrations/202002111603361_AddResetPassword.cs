namespace BlogSystem.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResetPassword : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ResetPasswords",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Token = c.String(),
                        UserId = c.Guid(nullable: false),
                        Email = c.String(),
                        IsSuccess = c.Boolean(nullable: false),
                        ExpireTime = c.DateTime(nullable: false),
                        CreatTime = c.DateTime(nullable: false),
                        IsRemoved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ResetPasswords", "UserId", "dbo.Users");
            DropIndex("dbo.ResetPasswords", new[] { "UserId" });
            DropTable("dbo.ResetPasswords");
        }
    }
}
