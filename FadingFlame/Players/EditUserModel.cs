using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FadingFlame.Players
{
    public class EditUserModel
    {
        [Required(ErrorMessageResourceName = "FieldRequired", ErrorMessageResourceType = typeof(FadingFlameTranslations))]
        public string DisplayName { get; set; }
        public string DiscordTag { get; set; }
    }
    
    public class EditListsModel
    {
        [Required(ErrorMessageResourceName = "FieldRequired", ErrorMessageResourceType = typeof(FadingFlameTranslations))]
        public string List1Name { get; set; }
        [Required(ErrorMessageResourceName = "FieldRequired", ErrorMessageResourceType = typeof(FadingFlameTranslations))]
        public string List1 { get; set; }
        [Required(ErrorMessageResourceName = "FieldRequired", ErrorMessageResourceType = typeof(FadingFlameTranslations))]
        public string List2Name { get; set; }
        [Required(ErrorMessageResourceName = "FieldRequired", ErrorMessageResourceType = typeof(FadingFlameTranslations))]
        public string List2 { get; set; }
        [Required]
        [Range(1,16, ErrorMessageResourceName = "FactionRequired", ErrorMessageResourceType = typeof(FadingFlameTranslations))]
        public Faction Faction { get; set; }
    }
}