using FluentMigrator;

namespace SocialNetwork.Data.Migrations
{
    [Migration(20210414205)]
    public class AddUserProfileTable : Migration
    {
        public override void Up()
        {
            Create.Table("UserProfile")
                .WithColumn("UserId").AsInt64().PrimaryKey().ForeignKey("User", "Id")
                .WithColumn("FirstName").AsString()
                .WithColumn("LastName").AsString()
                .WithColumn("Age").AsInt32()
                .WithColumn("Interests").AsString(int.MaxValue)
                .WithColumn("City").AsString();
        }

        public override void Down()
        {
            Delete.Table("UserProfile");
        }
    }
}