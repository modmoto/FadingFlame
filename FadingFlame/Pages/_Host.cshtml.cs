using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FadingFlame.Pages;

public class HostModel : PageModel
{
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
            RedirectUri = Url.Content("/"),
            AllowRefresh = true
        };
}