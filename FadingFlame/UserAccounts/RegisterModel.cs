using System.ComponentModel.DataAnnotations;

namespace FadingFlame.UserAccounts
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        public string DisplayName { get; set; }
    }
}