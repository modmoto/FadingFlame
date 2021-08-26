using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FadingFlame.Players
{
    public class EditUserModel
    {
        [Required]
        public string DisplayName { get; set; }
        public string DiscordTag { get; set; }
        public List<Faction> Armies { get; set; }
    }
    
    public class EditListsModel
    {
        [Required]
        public string List1Name { get; set; }
        [Required]
        public string List1 { get; set; }
        [Required]
        public string List2Name { get; set; }
        [Required]
        public string List2 { get; set; }
    }
}