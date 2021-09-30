using FadingFlame.Players.Pages;

namespace FadingFlame.Players
{
    public class EditUserModel
    {
        public string DiscordTag { get; set; }
        public string DisplayName { get; set; }
        public string TimeZone { get; set; }
        public string Country { get; set; }
    }
    
    public class EditListsModel
    {
        public GameListEditModel List1 { get; set; }
        public GameListEditModel List2 { get; set; }
    }

    public class Errors
    {
        public const string FactionRequired = "You have to select a faction";
        public const string FieldRequired = "This field is required";
    }
}