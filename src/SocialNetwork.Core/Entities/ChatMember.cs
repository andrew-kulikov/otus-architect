namespace SocialNetwork.Core.Entities
{
    public class ChatMember
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }

        public UserProfile UserProfile { get; set; }
    }
}