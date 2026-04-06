


using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Web.Models;
using MovieMvcProject.Web.ViewModelValidators;

namespace MovieMvcProject.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUserService _userService;           

        public HomeController(
            ILocalizationService localizationService,
            IUserService userService)                         
        {
            _localizationService = localizationService;
            _userService = userService;
        }

        
        public IActionResult Index()
        {
            
            return View(new EmailViewModel());
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmailViewModel model)
        {

            ModelState.Clear();

            
            var emailValidator = new EmailViewModelValidator(_localizationService);
            var validationResult = await emailValidator.ValidateAsync(model);

            
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                return View(model);
            }

            
            if (await _userService.IsEmailTakenAsync(model.Email))
            {
                ModelState.AddModelError("Email",
                    _localizationService.GetLocalizedHtmlString("UserResource", "UserAlreadyRegistered").Value
                    ?? "Bu e-posta adresi zaten sistemde kayıtlıdır.");

                return View(model);   
            }

            
            TempData["RegisterEmail"] = model.Email;
            TempData.Keep("RegisterEmail");

            return RedirectToAction("Register", "Account");
        }
    }
}