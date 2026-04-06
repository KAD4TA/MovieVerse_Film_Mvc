

using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Domain.Enums;
using MovieMvcProject.Domain.Identity;
using MovieMvcProject.Web.Models;
using System.Security.Claims;

namespace MovieMvcProject.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<AccountController> _logger;
        private readonly IFileService _fileService;
        private readonly IValidator<RegisterViewModel> _registerValidator;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IValidator<SettingsPageViewModel> _updateValidator;
        private readonly IValidator<LoginViewModel> _loginValidator;



        public AccountController(
    IMapper mapper,
    IUserService userService,
    ILocalizationService localizationService,
    IFileService fileService,
    ILogger<AccountController> logger,
    IValidator<RegisterViewModel> registerValidator,
    SignInManager<AppUser> signInManager,
        IValidator<LoginViewModel> loginValidator,
    IValidator<SettingsPageViewModel> updateValidator)
        {
            _mapper = mapper;
            _userService = userService;
            _localizationService = localizationService;
            _fileService = fileService;
            _logger = logger;
            _registerValidator = registerValidator;
            _signInManager = signInManager;
            _loginValidator = loginValidator;
            _updateValidator = updateValidator;
        }



        [HttpGet, AllowAnonymous]
        [EnableRateLimiting("login")]
        public IActionResult Login(string? returnUrl = null)
        {

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Movie");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }





        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dtoLogin = _mapper.Map<LoginRequestDto>(model);
                var result = await _userService.Login(dtoLogin);

                if (!result.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, result.Message ?? "E-posta veya şifre hatalı.");
                    return View(model);
                }

                TempData["Success"] = _localizationService.GetLocalizedString("LoginSuccess").Value ?? "Giriş başarılı!";


                return RedirectToAction("Index", "Movie");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login hatası");
                ModelState.AddModelError(string.Empty, "Sistem hatası oluştu, lütfen tekrar deneyin.");
                return View(model);
            }
        }


        [HttpGet, AllowAnonymous]
        [EnableRateLimiting("register")]
        public IActionResult Register([FromQuery] string? email = null)
        {
            var prefilledEmail = email ?? TempData["RegisterEmail"] as string ?? string.Empty;

            var model = new RegisterViewModel
            {
                Email = prefilledEmail,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                FullName = string.Empty,
                Gender = Gender.None,
                BirthDate = null
            };
            return View(model);
        }


        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        [EnableRateLimiting("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var dto = _mapper.Map<RegisterRequestDto>(model);
                var userResponse = await _userService.Register(dto);

                if (userResponse == null || string.IsNullOrEmpty(userResponse.Id))
                {
                    ModelState.AddModelError(string.Empty, _localizationService.GetLocalizedString("RegisterFailed").Value);
                    return View(model);
                }

                TempData["Success"] = _localizationService.GetLocalizedString("RegisterSuccess").Value;
                return RedirectToAction("Index", "Movie");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt hatası");


                ModelState.AddModelError("Email", ex.Message);
                TempData["Error"] = ex.Message;

                return View(model);
            }
        }



        [HttpGet("/Settings")]
        public async Task<IActionResult> Settings(string tab = "profile")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetByIdUser(userId);

            if (user == null)
            {
                _logger.LogWarning(
                    "Kullanıcı bulunamadığı için Settings sayfası yüklenemedi. ID: {UserId}",
                    userId
                );
                return RedirectToAction("Index", "Home");
            }

            var viewModel = _mapper.Map<SettingsPageViewModel>(user)
                           ?? new SettingsPageViewModel();

            viewModel.UserId = userId;
            viewModel.ActiveTab = string.IsNullOrWhiteSpace(tab) ? "profile" : tab;

            return View(viewModel);
        }


        [HttpPost("/Settings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsPageViewModel model)
        {

            var validationResult = await _updateValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var err in validationResult.Errors)
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);

                model.ActiveTab = model.ActiveTab ?? "profile";


                var previewUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(previewUserId))
                {
                    var currentUser = await _userService.GetByIdUser(previewUserId);
                    model.ProfileImage = currentUser?.ProfileImageUrl;
                }

                return View(model);
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");


            var dto = _mapper.Map<ProfileUpdateRequestDto>(model);
            dto.UserId = userId;




            var result = await _userService.UpdateProfile(dto);


            if (result == null || !result.IsSuccess)
            {

                ModelState.AddModelError(string.Empty, result?.Message ?? _localizationService.GetLocalizedString("UpdateFailed").Value ?? "Güncelleme başarısız.");
                model.ActiveTab = model.ActiveTab ?? "profile";

                var currentUser = await _userService.GetByIdUser(userId);
                model.ProfileImage = currentUser?.ProfileImageUrl;

                return View(model);
            }


            TempData["SuccessMessage"] = _localizationService.GetLocalizedString("ProfileUpdateSuccess").Value ?? "Ayarlar başarıyla güncellendi.";


            return RedirectToAction(nameof(Settings), new { tab = model.ActiveTab ?? "profile" });
        }




        [HttpGet("/User/PublicProfile/{id}")]
        public async Task<IActionResult> PublicProfile(string id, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var dto = await _userService.GetPublicProfileAsync(id, page, 12);

            if (dto == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index", "Home");
            }


            var model = _mapper.Map<UserPageViewModel>(dto);

            model.IsOwnProfile = User.Identity?.IsAuthenticated == true &&
                                 User.FindFirstValue(ClaimTypes.NameIdentifier) == id;

            return View(model);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                _ = await _userService.Logout(userId);

            await _signInManager.SignOutAsync();   // ← COOKIE TEMİZLENİYOR (EN ÖNEMLİ DÜZELTME)

            TempData["Success"] = _localizationService.GetLocalizedString("LogoutSuccess").Value ?? "Başarıyla çıkış yapıldı";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            try
            {
                var userResponse = await _userService.GetByIdUser(userId);
                var deleteResponse = await _userService.DeleteUser(userId);

                if (deleteResponse.IsSuccess)
                {
                    if (userResponse != null && !string.IsNullOrEmpty(userResponse.ProfileImageUrl))
                    {
                        await _fileService.DeleteProfileImageAsync(userResponse.ProfileImageUrl);
                    }
                    _ = await Logout();
                    TempData["Success"] = _localizationService.GetLocalizedString("AccountDeleteSuccess").Value;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = deleteResponse.Message ?? _localizationService.GetLocalizedString("AccountDeleteFailed").Value;
                    return RedirectToAction("Profile");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Silme hatası");
                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError").Value;
                return RedirectToAction("Profile");
            }
        }
    }


}
