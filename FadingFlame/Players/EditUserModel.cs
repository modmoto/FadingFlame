using System.ComponentModel.DataAnnotations;

namespace FadingFlame.Players
{
    public class EditUserModel
    {
        public string DisplayName { get; set; }
        public string DiscordTag { get; set; }
    }
    
    public class EditListsModel
    {
        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List1Name { get; set; }
        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List1 { get; set; }
        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List2Name { get; set; }
        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List2 { get; set; }
        [Required]
        [Range(1,16, ErrorMessage = Errors.FactionRequired)]
        public Faction Faction { get; set; }
    }

    public class Errors
    {
        public const string FactionRequired = "You have to select a faction";
        public const string FieldRequired = "This field is required";
    }
}