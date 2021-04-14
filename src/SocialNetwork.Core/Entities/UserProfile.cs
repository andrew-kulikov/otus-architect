namespace SocialNetwork.Core.Entities
{
    public class UserProfile
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
    }
}