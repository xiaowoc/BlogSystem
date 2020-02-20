namespace BlogSystem.Models.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BlogSystem.Models.BlogContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(BlogSystem.Models.BlogContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            //Ԥ��һ��idΪ�����˻���Ԥ��ϵͳ�趨�ķ���
            User user = new User()
            {
                Id= Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Email="all",
                Password="123456"
            };
            BlogCategory category = new BlogCategory()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                CategoryName = "�ö�",
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            };
            context.BlogCategorys.Add(category);
            context.Users.Add(user);
            base.Seed(context);
        }
    }
}
