using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discord.Oidc.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("login")]
        [Authorize(AuthenticationSchemes = DiscordAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult LoginAsync(string? returnUrl = null)
        {
            // Signed in because of Authorize attribute
            return Redirect(returnUrl ?? "/");
        }

        public async Task<ActionResult> LogoutAsync(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync();

            return Redirect(returnUrl ?? "/");
        }
    }
}
