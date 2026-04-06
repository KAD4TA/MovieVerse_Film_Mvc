

using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MovieMvcProject.Web.Controllers
{
    public class LanguageController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            
            if (string.IsNullOrEmpty(culture)) culture = "tr-TR";
            else if (culture == "tr") culture = "tr-TR";
            else if (culture == "en") culture = "en-US";

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            
            returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
            return LocalRedirect(returnUrl);
        }
    }
}