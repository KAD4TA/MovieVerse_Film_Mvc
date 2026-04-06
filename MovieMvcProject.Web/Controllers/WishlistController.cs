using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.Features.WishList.Commands;
using MovieMvcProject.Application.Features.WishList.Queries;
using MovieMvcProject.Web.Models;
using System.Security.Claims;

namespace MovieMvcProject.Web.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public WishlistController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }


        
        [HttpGet("/MyList")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 12)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var pagedDto = await _mediator.Send(new GetWishlistQuery(userId, pageNumber, pageSize));
            var pagedViewModel = new PagedResult<WishlistViewModel>(
            _mapper.Map<List<WishlistViewModel>>(pagedDto.Items),
            pagedDto.TotalCount,
            pagedDto.PageNumber,
            pagedDto.PageSize
            );
            return View(pagedViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid movieId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Geçersiz kullanıcı." });
            }
            var result = await _mediator.Send(new AddToWishlistCommand { MovieId = movieId, UserId = userId });
            if (result)
                return Json(new { success = true, message = "Film listenize eklendi." });
            return Json(new { success = false, message = "Bu film zaten listenizde." });
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid movieId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın." });
            }

            await _mediator.Send(new RemoveFromWishlistCommand(movieId, userId));

            return Json(new { success = true, message = "Film listenizden çıkarıldı." });
        }


    }
}
