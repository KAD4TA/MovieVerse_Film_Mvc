
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Features.Categories.Queries;
using MovieMvcProject.Application.Features.Comments.Commands;
using MovieMvcProject.Application.Features.Movies.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.VisitorTracking;
using MovieMvcProject.Web.Models;
using System.Globalization;
using System.Security.Claims;


namespace MovieMvcProject.Web.Controllers
{
    
    [Authorize]
    public class MovieController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<MovieController> _logger;
        private readonly IVisitorTrackingService _trackingService;

        
        public MovieController(IMediator mediator, IMapper mapper, IApplicationDbContext context,
                       ILogger<MovieController> logger,
            IVisitorTrackingService trackingService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _trackingService = trackingService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 12) 
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 40) pageSize = 12;

            var languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName ?? "tr";
            var query = new GetAllMoviesQuery(pageNumber, pageSize, languageCode);
            var result = await _mediator.Send(query);

            var totalPages = result.TotalPages;
            if (pageNumber > totalPages && totalPages > 0)
            {
                return RedirectToAction("Index", new { pageNumber = totalPages, pageSize });
            }

            return View(result);
        }

        
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term, int limit = 8)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName ?? "tr";

            
            var searchQuery = new SearchMoviesQuery(term, languageCode, pageNumber: 1, pageSize: limit);
            var result = await _mediator.Send(searchQuery);

            
            var suggestions = result.Items.Select(m => new
            {
                label = m.Year > 0 ? $"{m.Title} ({m.Year})" : $"{m.Title} (Bilinmiyor)",
                value = m.Title,
                poster = m.PosterUrl,
                url = $"/Movie/{m.MovieId}"                                 
            }).ToList();

            return Json(suggestions);
        }



        [HttpGet]
        public async Task<IActionResult> ExploreCategory(string category, int pageNumber = 1)
        {
            var langCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            //  Kategoriye göre filmleri getirme
            var query = new GetMoviesByCategoryQuery(langCode, category, pageNumber, 12);
            var result = await _mediator.Send(query);

            //  Haftalık trend filmleri getirme
            if (pageNumber == 1)
            {
                var trendingQuery = new GetWeeklyTrendingMoviesQuery(langCode, limit: 10);
                var trendingMovies = await _mediator.Send(trendingQuery);
                ViewBag.WeeklyTrending = trendingMovies;
            }

            return View(result);
        }



        [HttpGet]
        [Route("Movie/{id:guid}")]
        public async Task<IActionResult> MovieDetail(Guid id)
        {
            var query = new GetMovieByIdQuery(id);
            var movieDto = await _mediator.Send(query);

            if (movieDto == null) return NotFound();

            // ==================== ZİYARET KAYDI ====================
            var userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var agent = Request.Headers["User-Agent"].ToString() ?? "unknown";

            
            await _trackingService.TrackMovieVisitAsync(id, userId, ip, agent);
           

            var viewModel = _mapper.Map<MovieDetailViewModel>(movieDto);
            return View(viewModel);
        }

        // ----------------------------------------------------
        // 4. Yorum Ekleme (POST) 
        // ----------------------------------------------------
        [HttpPost]
        [Route("Movie/{id:guid}")]
        
        public async Task<IActionResult> MovieDetail(Guid id, [FromForm] MovieDetailViewModel model)
        {
            // Route'dan gelen ID'yi, formdan gelen NewComment nesnesine atıyoruz
            // Güvenlik ve veri bütünlüğü için önemlidir.
            model.NewComment.MovieId = id;

            
            if (!ModelState.IsValid)
            {
                

                var query = new GetMovieByIdQuery(id);
                var movieDto = await _mediator.Send(query);

                if (movieDto == null) return NotFound();

                
                var fullViewModel = _mapper.Map<MovieDetailViewModel>(movieDto);

                
                fullViewModel.NewComment = model.NewComment;

                
                return View(fullViewModel);
            }


            var command = _mapper.Map<CreateCommentCommand>(model.NewComment);
            await _mediator.Send(command);

            
            return RedirectToAction("MovieDetail", new { id = id });
        }
    }
}