namespace FadingFlame.UserAccounts
{
    public interface IUserContext
    {
        UserAccount LoggedInUser { get; set; }
    }

    public class UserContext : IUserContext
    {
        public UserAccount LoggedInUser { get; set; }
    }
}