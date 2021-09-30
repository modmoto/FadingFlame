using System.ComponentModel.DataAnnotations;

namespace FadingFlame.Players.Pages
{
    public class GameListEditModel
    {
        [Required(ErrorMessage = Errors.FieldRequired)]
        [Range(1, 20, ErrorMessage = Errors.FactionRequired)]
        public Faction Faction { get; set; }
        [Required(ErrorMessage = Errors.FieldRequired)]
        public string List { get; set; }
        public string ProposedListChange { get; set; }
        [Required(ErrorMessage = Errors.FieldRequired)]
        public string Name { get; set; }
    }
}