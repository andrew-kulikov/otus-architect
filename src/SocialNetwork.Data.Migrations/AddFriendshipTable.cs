using FluentMigrator;

namespace SocialNetwork.Data.Migrations
{
    [Migration(202104222251)]
    public class AddFriendshipTable : Migration
    {
        public override void Up()
        {
            Create.Table("Friendship")
                .WithColumn("RequesterId").AsInt64().PrimaryKey().ForeignKey("User", "Id")
                .WithColumn("AddresseeId").AsInt64().PrimaryKey().ForeignKey("User", "Id");
        }

        public override void Down()
        {
            Delete.Table("Friendship");
        }
    }
}