

using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Interfaces.ILocalization;
using System.Net;

namespace MovieMvcProject.Web.Controllers
{
   
    public class ErrorController : Controller
    {
        private readonly ILocalizationService _localization;

        public ErrorController(ILocalizationService localization)
        {
            _localization = localization;
        }

        private IActionResult ErrorView(int statusCode, string resourceKey, string? overrideMessage = null)
        {
            var defaultMessage = _localization.GetLocalizedHtmlString("ExceptionResource", resourceKey).Value;
            var title = _localization.GetLocalizedHtmlString("ExceptionResource", resourceKey)?.Value ?? defaultMessage;

            
            var finalMessage = overrideMessage ?? HttpContext.Items["ErrorMessage"]?.ToString() ?? defaultMessage;

            ViewBag.Title = title;
            ViewBag.StatusCode = statusCode;
            ViewBag.Message = finalMessage;

           
            return View($"~/Views/Error/Error{statusCode}.cshtml");
        }

        
        public IActionResult Error400(string? message = null)
        {
            if (!string.IsNullOrEmpty(message)) message = WebUtility.UrlDecode(message); 
            return ErrorView(400, "BadRequest", message);
        }

        
        public IActionResult Error401(string? message = null)
        {
            if (!string.IsNullOrEmpty(message)) message = WebUtility.UrlDecode(message);
            HttpContext.Items["ErrorMessage"] = message ?? HttpContext.Items["ErrorMessage"];
            return ErrorView(401, "Unauthorized");
        }

        
        public IActionResult Error403(string? message = null)
        {
            if (!string.IsNullOrEmpty(message)) message = WebUtility.UrlDecode(message);
            return ErrorView(403, "Forbidden", message);
        }

        
        public IActionResult Error404(string? message = null)
        {
            if (!string.IsNullOrEmpty(message)) message = WebUtility.UrlDecode(message);
            return ErrorView(404, "NotFound", message);
        }

       
        public IActionResult Error409(string? message = null)
        {
            if (!string.IsNullOrEmpty(message)) message = WebUtility.UrlDecode(message);
            return ErrorView(409, "Conflict", message);
        }

        
        public IActionResult Error500(string? message = null)
        {
            if (!string.IsNullOrEmpty(message)) message = WebUtility.UrlDecode(message);
            return ErrorView(500, "ServerError", message);
        }
    }
}
