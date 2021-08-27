using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FadingFlame.UserAccounts
{
    public class HostAuthModel : PageModel
    {
        private readonly UserState _userState;

        public HostAuthModel(UserState userState)
        {
            _userState = userState;
        }
        public IActionResult OnGetLogin()
        {
            return Challenge(AuthProps(), "oidc");
        }

        public async Task OnGetLogout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc", AuthProps());
        }

        private AuthenticationProperties AuthProps()
            => new AuthenticationProperties
            {
                RedirectUri = Url.Content("/")
            };
    }
}