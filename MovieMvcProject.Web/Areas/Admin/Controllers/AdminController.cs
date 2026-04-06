using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.LiveSearch;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Actors.Commands;
using MovieMvcProject.Application.Features.LiveSearch.Queries;
using MovieMvcProject.Application.Features.Movies.Commands;
using MovieMvcProject.Application.Features.Movies.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Web.Areas.Admin.Models;

namespace MovieMvcProject.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin,moderator")]
    [EnableRateLimiting("heavy-db")]
    public class AdminController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IMediator mediator,
            IUserService userService,
            IMapper mapper,
            ILocalizationService localizationService,
            ILogger<AdminController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpGet]
        public async Task<IActionResult> ActorSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Json(Array.Empty<object>());

            try
            {
                var actorResults = await _mediator.Send(new GetActorSearchQuery(query, 8));

                var results = actorResults.Select(a => new
                {
                    id = a.Id,
                    name = a.Title,
                    photoUrl = a.PhotoUrl
                }).ToList();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oyuncu arama hatası");
                return Json(Array.Empty<object>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> DirectorSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Json(new List<object>());

            try
            {
                var directorResults = await _mediator.Send(new GetDirectorSearchQuery(query, 8));


                var results = directorResults.Select(d => new
                {
                    id = d.Id,
                    name = d.Title,
                    photoUrl = d.PhotoUrl
                }).ToList();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yönetmen arama hatası");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> LiveSearch([FromQuery] string query)
        {

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { success = true, items = new List<LiveSearchResultDto>() });
            }

            try
            {
                const int MAX_RESULTS_PER_TYPE = 4;
                var searchResults = new List<LiveSearchResultDto>();

                string languageCode = _localizationService.GetCurrentLanguageCode();

                // 2. 🎬 FİLM ARAMASI
                try
                {
                    // Dil kodunu buraya parametre olarak geçiyoruz
                    var movieSearchQuery = new SearchMoviesQuery(query, languageCode, pageNumber: 1, pageSize: MAX_RESULTS_PER_TYPE);

                    var movieResponse = await _mediator.Send(movieSearchQuery);

                    if (movieResponse?.Items != null)
                    {
                        searchResults.AddRange(movieResponse.Items.Select(m => new LiveSearchResultDto(
                            Id: m.MovieId.ToString(),
                            Title: m.Title, // Mediator zaten dile göre Title getirecek
                            Type: "Film",
                            Url: Url.Action(nameof(EditMovie), "Admin", new { id = m.MovieId })!,
                            PhotoUrl: m.PosterUrl
                        )));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Elasticsearch/Film araması başarısız oldu. Query: {Query}", query);
                }

                //  KULLANICI ARAMASI
                var userSearchQuery = new GetUserSearchQuery(query, MAX_RESULTS_PER_TYPE);
                var userResults = await _mediator.Send(userSearchQuery);
                if (userResults != null) searchResults.AddRange(userResults);

                // OYUNCU ARAMASI
                var actorSearchQuery = new GetActorSearchQuery(query, MAX_RESULTS_PER_TYPE);
                var actorResults = await _mediator.Send(actorSearchQuery);
                if (actorResults != null) searchResults.AddRange(actorResults);

                var directorSearchQuery = new GetDirectorSearchQuery(query, MAX_RESULTS_PER_TYPE);
                var directorResults = await _mediator.Send(directorSearchQuery);
                if (directorResults != null) searchResults.AddRange(directorResults);

                //  SONUÇLARI SINIRLAMA VE DÖNDÜRME
                var finalResults = searchResults.Take(MAX_RESULTS_PER_TYPE * 3).ToList();

                return Json(new { success = true, items = finalResults });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Canlı arama sırasında beklenmedik hata oluştu: {Query}", query);
                return Json(new { success = false, message = "Arama servisi kullanılamıyor." });
            }
        }



        [HttpGet("/Admin/Admin/MovieList/")]
        public async Task<IActionResult> MovieList(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string cb = null)
        {
            try
            {

                string languageCode = _localizationService.GetCurrentLanguageCode();

                PagedResult<MovieDtoResponse> result;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchQuery = new SearchMoviesQuery(searchTerm, languageCode, pageNumber, pageSize);
                    result = await _mediator.Send(searchQuery);
                    ViewBag.SearchTerm = searchTerm;
                }
                else
                {
                    var query = new GetAllMoviesQuery(pageNumber, pageSize, languageCode);
                    result = await _mediator.Send(query);
                }

                var viewModelItems = _mapper.Map<List<MovieViewModel>>(result.Items);
                var pagedViewModel = new PagedResult<MovieViewModel>(
                    viewModelItems,
                    result.TotalCount,
                    pageNumber,
                    pageSize);

                return View(pagedViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Film listesi getirilirken hata oluştu.");
                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError").Value;
                return View(new PagedResult<MovieViewModel>(new List<MovieViewModel>(), 0, pageNumber, pageSize));
            }
        }

        [HttpPost("/Admin/Admin/ToggleSlider")]

        public async Task<IActionResult> ToggleSlider([FromBody] ToggleSliderRequest? request)
        {
            if (request == null || request.MovieId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "Geçersiz istek verisi." });
            }


            var command = new UpdateMovieSliderStatusCommand(request.MovieId, request.IsOnSlider);
            var result = await _mediator.Send(command);

            if (result)
            {
                return Ok(new { success = true, message = "Slider durumu başarıyla güncellendi." });
            }

            return NotFound(new { success = false, message = "Film bulunamadı veya güncelleme yapılamadı." });
        }


        [HttpGet]
        public IActionResult CreateMovie()
        {
            return View(new MovieCreateUpdateViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovie(MovieCreateUpdateViewModel model)
        {

            if (!string.IsNullOrEmpty(model.DirectorName))
                ModelState.Remove("ExistingDirectorId");

            if (!ModelState.IsValid)
            {
                model.Actors ??= new List<ActorViewModel>();
                return View(model);
            }

            try
            {

                var createDto = _mapper.Map<CreateMovieDto>(model);

                var command = new CreateMovieCommand(createDto);
                await _mediator.Send(command);


                return RedirectToAction(nameof(MovieList), new
                {
                    cb = Guid.NewGuid(),
                    success = "true",
                    msg = _localizationService.GetLocalizedString("MovieCreateSuccess")?.Value
                          ?? "Film başarıyla kaydedildi."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Film oluşturma hatası. Model: {@Model}", model);

                ModelState.AddModelError("", "Film kaydedilirken bir hata oluştu.");
                model.Actors ??= new List<ActorViewModel>();
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditMovie(Guid id)
        {

            var movieDto = await _mediator.Send(new GetMovieForUpdateQuery(id));

            if (movieDto == null)
            {
                _logger.LogWarning("{MovieId} ID'li film güncelleme için bulunamadı.", id);
                return NotFound();
            }


            var viewModel = _mapper.Map<MovieCreateUpdateViewModel>(movieDto);


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovie(Guid id, MovieCreateUpdateViewModel model)
        {

            model.MovieId = id;

            if (!ModelState.IsValid)
            {

                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Validation Hatası: {ErrorMessage}", error.ErrorMessage);
                    }
                }
                return View(model);
            }

            try
            {

                var updateDto = _mapper.Map<UpdateMovieDto>(model);


                var result = await _mediator.Send(new UpdateMovieCommand(updateDto));

                if (result != null)
                {
                    TempData["Success"] = "Film başarıyla güncellendi.";
                    return RedirectToAction(nameof(MovieList));
                }


                ModelState.AddModelError("", "Güncelleme işlemi başarısız oldu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditMovie POST işlemi sırasında hata oluştu. MovieId: {MovieId}", id);
                ModelState.AddModelError("", "Sistemde bir hata oluştu: " + ex.Message);
            }


            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> CreateActor(CreateActorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ViewModel'i doğrudan Command'e çeviriyoruz
            var command = _mapper.Map<CreateActorCommand>(model);
            var actorId = await _mediator.Send(command);

            return RedirectToAction("Actors");
        }



        [HttpGet]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            if (id == Guid.Empty)
                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });

            try
            {
                var query = new GetMovieByIdQuery(id);
                var movieDto = await _mediator.Send(query);
                var viewModel = _mapper.Map<MovieViewModel>(movieDto);
                return View(viewModel);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = _localizationService.GetLocalizedString("MovieNotFound").Value;
                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Silme onayı sayfası yüklenirken hata: {Id}", id);
                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError").Value;
                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
            }
        }

        [HttpPost, ActionName("DeleteMovie")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovieConfirmed(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["Error"] = _localizationService.GetLocalizedString("InvalidMovieId").Value;
                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
            }

            try
            {
                var deleteDto = new DeleteMovieDto { MovieId = id };
                var command = new DeleteMovieCommand(deleteDto);
                var result = await _mediator.Send(command);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Film silme hatası: {Id}", id);
                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError").Value;
            }


            return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> UserList(string searchTerm = "", int page = 1)
        {

            var response = await _userService.GetAllUsers(searchTerm, page, 10);

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Message;
                return View(new List<UserViewModel>());
            }


            ViewBag.TotalUserCount = response.TotalCount;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = response.PageNumber;
            ViewBag.TotalPages = response.TotalPages;

            var viewModel = _mapper.Map<List<UserViewModel>>(response.Users);
            return View(viewModel);
        }


        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("UserList");

            var userDto = await _userService.GetByIdUser(id);
            if (userDto == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("UserList");
            }

            var model = _mapper.Map<UserUpdateViewModel>(userDto);


            if (string.IsNullOrEmpty(model.ProfileImageUrl))
                model.ProfileImageUrl = "/profile-images/default-profile.png";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserUpdateViewModel model)
        {

            if (!ModelState.IsValid)
            {

                if (string.IsNullOrEmpty(model.ProfileImageUrl))
                    model.ProfileImageUrl = "/profile-images/default-profile.png";
                return View(model);
            }

            var updateDto = _mapper.Map<ProfileUpdateRequestDto>(model);

            try
            {
                var result = await _userService.UpdateProfile(updateDto);

                if (result.IsSuccess)
                {
                    TempData["Success"] = $"Kullanıcı başarıyla güncellendi ({result.Message}).";
                    return RedirectToAction(nameof(UserList));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Güncelleme sırasında beklenmedik sistem hatası oluştu. Kullanıcı ID: {UserId}", updateDto.UserId);

                ModelState.AddModelError(string.Empty, "Beklenmedik bir sistem hatası oluştu. Lütfen Logları kontrol edin.");

                return View(model);
            }
        }


        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(UserList));

            try
            {
                var userDto = await _userService.GetByIdUser(id);
                if (userDto == null)
                {
                    TempData["Error"] = _localizationService.GetLocalizedString("UserNotFound")?.Value ?? "Kullanıcı bulunamadı.";
                    return RedirectToAction(nameof(UserList));
                }

                var viewModel = _mapper.Map<Areas.Admin.Models.UserViewModel>(userDto);

                // Profil resmi fallback
                if (string.IsNullOrEmpty(viewModel.UserPhoto))
                    viewModel.UserPhoto = "/profile-images/default-profile.png";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı silme onayı sayfası yüklenirken hata. UserId: {Id}", id);
                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError")?.Value ?? "Beklenmedik bir hata oluştu.";
                return RedirectToAction(nameof(UserList));
            }
        }
        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Geçersiz kullanıcı ID.";
                return RedirectToAction(nameof(UserList));
            }

            // Admin kendi hesabını silemez
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (id == currentUserId)
            {
                TempData["Error"] = "Kendi hesabınızı silemezsiniz.";
                return RedirectToAction(nameof(UserList));
            }

            try
            {
                var result = await _userService.DeleteUser(id);

                if (result.IsSuccess)
                    TempData["Success"] = result.Message ?? "Kullanıcı başarıyla silindi.";
                else
                    TempData["Error"] = result.Message ?? "Kullanıcı silinirken bir hata oluştu.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı silme hatası. UserId: {Id}", id);
                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError")?.Value ?? "Beklenmedik bir hata oluştu.";
            }

            return RedirectToAction(nameof(UserList));
        }
    }
}


//using AutoMapper;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.RateLimiting;
//using MovieMvcProject.Application.Commons;
//using MovieMvcProject.Application.Commons.Exceptions;
//using MovieMvcProject.Application.DTOs.LiveSearch;
//using MovieMvcProject.Application.DTOs.RequestDto;
//using MovieMvcProject.Application.DTOs.ResponseDto;
//using MovieMvcProject.Application.Features.Actors.Commands;
//using MovieMvcProject.Application.Features.LiveSearch.Queries;
//using MovieMvcProject.Application.Features.Movies.Commands;
//using MovieMvcProject.Application.Features.Movies.Queries;
//using MovieMvcProject.Application.Interfaces;
//using MovieMvcProject.Application.Interfaces.ILocalization;
//using MovieMvcProject.Web.Areas.Admin.Models;

//namespace MovieMvcProject.Web.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    [Authorize(Roles = "admin,moderator")]
//    [EnableRateLimiting("heavy-db")]
//    public class AdminController : Controller
//    {
//        private readonly IMediator _mediator;
//        private readonly IUserService _userService;
//        private readonly IMapper _mapper;
//        private readonly ILocalizationService _localizationService;
//        private readonly ILogger<AdminController> _logger;

//        public AdminController(
//            IMediator mediator,
//            IUserService userService,
//            IMapper mapper,
//            ILocalizationService localizationService,
//            ILogger<AdminController> logger)
//        {
//            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
//            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
//            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
//            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }


//        [HttpGet]
//        public async Task<IActionResult> ActorSearch([FromQuery] string query)
//        {
//            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
//                return Json(Array.Empty<object>());

//            try
//            {
//                var actorResults = await _mediator.Send(new GetActorSearchQuery(query, 8));

//                var results = actorResults.Select(a => new
//                {
//                    id = a.Id,
//                    name = a.Title,
//                    photoUrl = a.PhotoUrl
//                }).ToList();

//                return Json(results);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Oyuncu arama hatası");
//                return Json(Array.Empty<object>());
//            }
//        }


//        [HttpGet]
//        public async Task<IActionResult> DirectorSearch([FromQuery] string query)
//        {
//            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
//                return Json(new List<object>());

//            try
//            {
//                var directorResults = await _mediator.Send(new GetDirectorSearchQuery(query, 8));


//                var results = directorResults.Select(d => new
//                {
//                    id = d.Id,
//                    name = d.Title,
//                    photoUrl = d.PhotoUrl
//                }).ToList();

//                return Json(results);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Yönetmen arama hatası");
//                return Json(new List<object>());
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> LiveSearch([FromQuery] string query)
//        {

//            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
//            {
//                return Json(new { success = true, items = new List<LiveSearchResultDto>() });
//            }

//            try
//            {
//                const int MAX_RESULTS_PER_TYPE = 4;
//                var searchResults = new List<LiveSearchResultDto>();

//                string languageCode = _localizationService.GetCurrentLanguageCode();

//                // 2. 🎬 FİLM ARAMASI
//                try
//                {
//                    // Dil kodunu buraya parametre olarak geçiyoruz
//                    var movieSearchQuery = new SearchMoviesQuery(query, languageCode, pageNumber: 1, pageSize: MAX_RESULTS_PER_TYPE);

//                    var movieResponse = await _mediator.Send(movieSearchQuery);

//                    if (movieResponse?.Items != null)
//                    {
//                        searchResults.AddRange(movieResponse.Items.Select(m => new LiveSearchResultDto(
//                            Id: m.MovieId.ToString(),
//                            Title: m.Title, // Mediator zaten dile göre Title getirecek
//                            Type: "Film",
//                            Url: Url.Action(nameof(EditMovie), "Admin", new { id = m.MovieId })!,
//                            PhotoUrl: m.PosterUrl
//                        )));
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogWarning(ex, "Elasticsearch/Film araması başarısız oldu. Query: {Query}", query);
//                }

//                //  KULLANICI ARAMASI
//                var userSearchQuery = new GetUserSearchQuery(query, MAX_RESULTS_PER_TYPE);
//                var userResults = await _mediator.Send(userSearchQuery);
//                if (userResults != null) searchResults.AddRange(userResults);

//                // OYUNCU ARAMASI
//                var actorSearchQuery = new GetActorSearchQuery(query, MAX_RESULTS_PER_TYPE);
//                var actorResults = await _mediator.Send(actorSearchQuery);
//                if (actorResults != null) searchResults.AddRange(actorResults);

//                var directorSearchQuery = new GetDirectorSearchQuery(query, MAX_RESULTS_PER_TYPE);
//                var directorResults = await _mediator.Send(directorSearchQuery);
//                if (directorResults != null) searchResults.AddRange(directorResults);

//                //  SONUÇLARI SINIRLAMA VE DÖNDÜRME
//                var finalResults = searchResults.Take(MAX_RESULTS_PER_TYPE * 3).ToList();

//                return Json(new { success = true, items = finalResults });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Canlı arama sırasında beklenmedik hata oluştu: {Query}", query);
//                return Json(new { success = false, message = "Arama servisi kullanılamıyor." });
//            }
//        }



//        [HttpGet("/Admin/Admin/MovieList/")]
//        public async Task<IActionResult> MovieList(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string cb = null)
//        {
//            try
//            {

//                string languageCode = _localizationService.GetCurrentLanguageCode();

//                PagedResult<MovieDtoResponse> result;

//                if (!string.IsNullOrWhiteSpace(searchTerm))
//                {
//                    var searchQuery = new SearchMoviesQuery(searchTerm, languageCode, pageNumber, pageSize);
//                    result = await _mediator.Send(searchQuery);
//                    ViewBag.SearchTerm = searchTerm;
//                }
//                else
//                {
//                    var query = new GetAllMoviesQuery(pageNumber, pageSize, languageCode);
//                    result = await _mediator.Send(query);
//                }

//                var viewModelItems = _mapper.Map<List<MovieViewModel>>(result.Items);
//                var pagedViewModel = new PagedResult<MovieViewModel>(
//                    viewModelItems,
//                    result.TotalCount,
//                    pageNumber,
//                    pageSize);

//                return View(pagedViewModel);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Film listesi getirilirken hata oluştu.");
//                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError").Value;
//                return View(new PagedResult<MovieViewModel>(new List<MovieViewModel>(), 0, pageNumber, pageSize));
//            }
//        }

//        [HttpPost("/Admin/Admin/ToggleSlider")]

//        public async Task<IActionResult> ToggleSlider([FromBody] ToggleSliderRequest? request)
//        {
//            if (request == null || request.MovieId == Guid.Empty)
//            {
//                return BadRequest(new { success = false, message = "Geçersiz istek verisi." });
//            }


//            var command = new UpdateMovieSliderStatusCommand(request.MovieId, request.IsOnSlider);
//            var result = await _mediator.Send(command);

//            if (result)
//            {
//                return Ok(new { success = true, message = "Slider durumu başarıyla güncellendi." });
//            }

//            return NotFound(new { success = false, message = "Film bulunamadı veya güncelleme yapılamadı." });
//        }


//        [HttpGet]
//        public IActionResult CreateMovie()
//        {
//            return View(new MovieCreateUpdateViewModel());
//        }

//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> CreateMovie(MovieCreateUpdateViewModel model)
//        {

//            if (!string.IsNullOrEmpty(model.DirectorName))
//                ModelState.Remove("ExistingDirectorId");

//            if (!ModelState.IsValid)
//            {
//                model.Actors ??= new List<ActorViewModel>();
//                return View(model);
//            }

//            try
//            {

//                var createDto = _mapper.Map<CreateMovieDto>(model);

//                var command = new CreateMovieCommand(createDto);
//                await _mediator.Send(command);


//                return RedirectToAction(nameof(MovieList), new
//                {
//                    cb = Guid.NewGuid(),
//                    success = "true",
//                    msg = _localizationService.GetLocalizedString("MovieCreateSuccess")?.Value
//                          ?? "Film başarıyla kaydedildi."
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Film oluşturma hatası. Model: {@Model}", model);

//                ModelState.AddModelError("", "Film kaydedilirken bir hata oluştu.");
//                model.Actors ??= new List<ActorViewModel>();
//                return View(model);
//            }
//        }


//        [HttpGet]
//        public async Task<IActionResult> EditMovie(Guid id)
//        {

//            var movieDto = await _mediator.Send(new GetMovieForUpdateQuery(id));

//            if (movieDto == null)
//            {
//                _logger.LogWarning("{MovieId} ID'li film güncelleme için bulunamadı.", id);
//                return NotFound();
//            }


//            var viewModel = _mapper.Map<MovieCreateUpdateViewModel>(movieDto);


//            return View(viewModel);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> EditMovie(Guid id, MovieCreateUpdateViewModel model)
//        {

//            model.MovieId = id;

//            if (!ModelState.IsValid)
//            {

//                foreach (var modelState in ModelState.Values)
//                {
//                    foreach (var error in modelState.Errors)
//                    {
//                        _logger.LogError("Validation Hatası: {ErrorMessage}", error.ErrorMessage);
//                    }
//                }
//                return View(model);
//            }

//            try
//            {

//                var updateDto = _mapper.Map<UpdateMovieDto>(model);


//                var result = await _mediator.Send(new UpdateMovieCommand(updateDto));

//                if (result != null)
//                {
//                    TempData["Success"] = "Film başarıyla güncellendi.";
//                    return RedirectToAction(nameof(MovieList));
//                }


//                ModelState.AddModelError("", "Güncelleme işlemi başarısız oldu.");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "EditMovie POST işlemi sırasında hata oluştu. MovieId: {MovieId}", id);
//                ModelState.AddModelError("", "Sistemde bir hata oluştu: " + ex.Message);
//            }


//            return View(model);
//        }


//        [HttpPost]
//        public async Task<IActionResult> CreateActor(CreateActorViewModel model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            // ViewModel'i doğrudan Command'e çeviriyoruz
//            var command = _mapper.Map<CreateActorCommand>(model);
//            var actorId = await _mediator.Send(command);

//            return RedirectToAction("Actors");
//        }



//        [HttpGet]
//        public async Task<IActionResult> DeleteMovie(Guid id)
//        {
//            if (id == Guid.Empty)
//                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });

//            try
//            {
//                var query = new GetMovieByIdQuery(id);
//                var movieDto = await _mediator.Send(query);
//                var viewModel = _mapper.Map<MovieViewModel>(movieDto);
//                return View(viewModel);
//            }
//            catch (NotFoundException)
//            {
//                TempData["Error"] = _localizationService.GetLocalizedString("MovieNotFound").Value;
//                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Silme onayı sayfası yüklenirken hata: {Id}", id);
//                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError").Value;
//                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
//            }
//        }

//        [HttpPost, ActionName("DeleteMovie")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteMovieConfirmed(Guid id)
//        {
//            if (id == Guid.Empty)
//            {
//                TempData["Error"] = _localizationService.GetLocalizedString("InvalidMovieId").Value;
//                return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
//            }

//            try
//            {
//                var deleteDto = new DeleteMovieDto { MovieId = id };
//                var command = new DeleteMovieCommand(deleteDto);
//                var result = await _mediator.Send(command);

//                if (result.Success)
//                    TempData["Success"] = result.Message;
//                else
//                    TempData["Error"] = result.Message;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Film silme hatası: {Id}", id);
//                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError").Value;
//            }


//            return RedirectToAction(nameof(MovieList), new { cb = Guid.NewGuid() });
//        }

//        [Authorize(Roles = "admin")]
//        [HttpGet]
//        public async Task<IActionResult> UserList(string searchTerm = "", int page = 1)
//        {

//            var response = await _userService.GetAllUsers(searchTerm, page, 10);

//            if (!response.IsSuccess)
//            {
//                TempData["Error"] = response.Message;
//                return View(new List<UserViewModel>());
//            }


//            ViewBag.TotalUserCount = response.TotalCount;
//            ViewBag.SearchTerm = searchTerm;
//            ViewBag.CurrentPage = response.PageNumber;
//            ViewBag.TotalPages = response.TotalPages;

//            var viewModel = _mapper.Map<List<UserViewModel>>(response.Users);
//            return View(viewModel);
//        }


//        [Authorize(Roles = "admin")]
//        [HttpGet]
//        public async Task<IActionResult> EditUser(string id)
//        {
//            if (string.IsNullOrEmpty(id))
//                return RedirectToAction("UserList");

//            var userDto = await _userService.GetByIdUser(id);
//            if (userDto == null)
//            {
//                TempData["Error"] = "Kullanıcı bulunamadı.";
//                return RedirectToAction("UserList");
//            }

//            var model = _mapper.Map<UserUpdateViewModel>(userDto);


//            if (string.IsNullOrEmpty(model.ProfileImageUrl))
//                model.ProfileImageUrl = "/profile-images/default-profile.png";

//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> EditUser(UserUpdateViewModel model)
//        {

//            if (!ModelState.IsValid)
//            {

//                if (string.IsNullOrEmpty(model.ProfileImageUrl))
//                    model.ProfileImageUrl = "/profile-images/default-profile.png";
//                return View(model);
//            }

//            var updateDto = _mapper.Map<ProfileUpdateRequestDto>(model);

//            try
//            {
//                var result = await _userService.UpdateProfile(updateDto);

//                if (result.IsSuccess)
//                {
//                    TempData["Success"] = $"Kullanıcı başarıyla güncellendi ({result.Message}).";
//                    return RedirectToAction(nameof(UserList));
//                }
//                else
//                {
//                    ModelState.AddModelError(string.Empty, result.Message);
//                    return View(model);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Güncelleme sırasında beklenmedik sistem hatası oluştu. Kullanıcı ID: {UserId}", updateDto.UserId);

//                ModelState.AddModelError(string.Empty, "Beklenmedik bir sistem hatası oluştu. Lütfen Logları kontrol edin.");

//                return View(model);
//            }
//        }


//        [Authorize(Roles = "admin")]
//        [HttpGet]
//        public async Task<IActionResult> DeleteUser(string id)
//        {
//            if (string.IsNullOrEmpty(id))
//                return RedirectToAction(nameof(UserList));

//            try
//            {
//                var userDto = await _userService.GetByIdUser(id);
//                if (userDto == null)
//                {
//                    TempData["Error"] = _localizationService.GetLocalizedString("UserNotFound")?.Value ?? "Kullanıcı bulunamadı.";
//                    return RedirectToAction(nameof(UserList));
//                }

//                var viewModel = _mapper.Map<Areas.Admin.Models.UserViewModel>(userDto);

//                // Profil resmi fallback
//                if (string.IsNullOrEmpty(viewModel.UserPhoto))
//                    viewModel.UserPhoto = "/profile-images/default-profile.png";

//                return View(viewModel);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Kullanıcı silme onayı sayfası yüklenirken hata. UserId: {Id}", id);
//                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError")?.Value ?? "Beklenmedik bir hata oluştu.";
//                return RedirectToAction(nameof(UserList));
//            }
//        }
//        [Authorize(Roles = "admin")]
//        [HttpPost, ActionName("DeleteUser")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteUserConfirmed(string id)
//        {
//            if (string.IsNullOrEmpty(id))
//            {
//                TempData["Error"] = "Geçersiz kullanıcı ID.";
//                return RedirectToAction(nameof(UserList));
//            }

//            // Admin kendi hesabını silemez
//            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
//            if (id == currentUserId)
//            {
//                TempData["Error"] = "Kendi hesabınızı silemezsiniz.";
//                return RedirectToAction(nameof(UserList));
//            }

//            try
//            {
//                var result = await _userService.DeleteUser(id);

//                if (result.IsSuccess)
//                    TempData["Success"] = result.Message ?? "Kullanıcı başarıyla silindi.";
//                else
//                    TempData["Error"] = result.Message ?? "Kullanıcı silinirken bir hata oluştu.";
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Kullanıcı silme hatası. UserId: {Id}", id);
//                TempData["Error"] = _localizationService.GetLocalizedString("UnexpectedError")?.Value ?? "Beklenmedik bir hata oluştu.";
//            }

//            return RedirectToAction(nameof(UserList));
//        }
//    }
//}
