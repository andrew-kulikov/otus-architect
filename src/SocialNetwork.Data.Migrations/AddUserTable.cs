using FluentMigrator;

namespace SocialNetwork.Data.Migrations
{
    [Migration(202104142105)]
    public class AddUserTable : Migration
    {
        public override void Up()
        {
            Create.Table("User")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Username").AsString().Unique().NotNullable()
                .WithColumn("Email").AsString().Unique().NotNullable()
                .WithColumn("PasswordHash").AsString().NotNullable()
                .WithColumn("RegisteredAt").AsDateTime().NotNullable()
                .WithColumn("RegistrationCompleted").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Table("User");
        }
    }
}