using Microsoft.AspNetCore.Mvc;

namespace MovieMvcProject.Web.Controllers
{
    public class InfoController : Controller
    {

        public IActionResult Page(string slug)
        {
            ViewData["Slug"] = slug; // "About", "KVKK", "Privacy"
            return View();
        }
    }
}
