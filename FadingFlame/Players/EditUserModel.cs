using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FadingFlame.Players
{
    public class EditUserModel
    {
        public string DiscordTag { get; set; }
        public string DisplayName { get; set; }
        public Location Location { get; set; }
    }
    
    public class EditListsModel
    {
        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List1Name { get; set; }

        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List1 { get; set; }

        [Required(ErrorMessage = Errors.FieldRequired)]
        [Range(1, 20, ErrorMessage = Errors.FactionRequired)]
        public Faction Faction1 { get; set; }

        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List2Name { get; set; }

        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List2 { get; set; }

        [Required(ErrorMessage = Errors.FieldRequired)]
        [Range(1, 20, ErrorMessage = Errors.FactionRequired)]
        public Faction Faction2 { get; set; }
    }

    public class Errors
    {
        public const string FactionRequired = "You have to select a faction";
        public const string FieldRequired = "This field is required";
    }
}