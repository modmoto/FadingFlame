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
}