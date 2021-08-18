using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.UserAccounts
{
    public class UserAccount
    {
        public ObjectId PlayerId { get; set; }
        [BsonId]
        public string Email { get; set; }
        public string Password { get; set; }

        public static UserAccount Create(ObjectId playerId, string email, string password)
        {
            return new()
            {
                PlayerId = playerId,
                Email = email,
                Password = password
            };
        }
    }
}