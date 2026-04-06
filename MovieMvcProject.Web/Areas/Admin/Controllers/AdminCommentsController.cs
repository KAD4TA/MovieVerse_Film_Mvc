using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Comments.Commands;
using MovieMvcProject.Application.Features.Comments.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Web.Areas.Admin.Controllers
{
   


    [Area("Admin")]
    [Authorize(Roles = "admin,moderator")]
    [Route("Admin/[controller]/[action]")]
    public class AdminCommentsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;   

        public AdminCommentsController(IMediator mediator, IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Index(int pageNumber = 1, string searchTerm = "", CommentStatus? status = null)
        {
            if (pageNumber < 1) pageNumber = 1;

            PagedResult<CommentDtoResponse> result;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                
                var searchQuery = new SearchCommentsQuery
                {
                    Query = searchTerm.Trim(),
                    PageNumber = pageNumber,
                    PageSize = 15
                };
                result = await _mediator.Send(searchQuery);
                ViewBag.SelectedStatus = null; 
            }
            else
            {
                
                var getAllQuery = new GetAllCommentsQuery
                {
                    PageNumber = pageNumber,
                    PageSize = 15,
                    SearchTerm = searchTerm,
                    Status = status
                };
                result = await _mediator.Send(getAllQuery);
                ViewBag.SelectedStatus = status;
            }

            ViewBag.SearchTerm = searchTerm;
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickApprove(Guid id)
        {
            await _mediator.Send(new UpdateCommentStatusCommand
            {
                CommentId = id,
                NewStatus = CommentStatus.Approved
            });
            return Json(new { success = true, message = "Yorum onaylandı." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickReject(Guid id)
        {
            await _mediator.Send(new UpdateCommentStatusCommand
            {
                CommentId = id,
                NewStatus = CommentStatus.Rejected
            });
            return Json(new { success = true, message = "Yorum reddedildi." });
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingComments()
        {
            

            var pendingEntities = await _unitOfWork.Comments.FindAsync(
                c => c.Status == CommentStatus.Pending);

            var pendingComments = pendingEntities
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new
                {
                    id = c.CommentId,
                    userName = c.User != null ? c.User.FullName : "Anonim",
                    userImage = (c.User != null && !string.IsNullOrEmpty(c.User.ProfileImageUrl))
                                ? c.User.ProfileImageUrl
                                : "/profile-images/default-profile.png",
                    movieName = c.Movie != null
                                ? (c.Movie.Translations?.FirstOrDefault(t => t.LanguageCode == "tr")?.Title
                                   ?? c.Movie.TitleTr ?? "Film Bilgisi Yok")
                                : "Film Bilgisi Yok",
                    content = c.Content,
                    date = c.CreatedAt
                })
                .ToList();

            var totalCount = await _unitOfWork.Comments.CountAsync(c => c.Status == CommentStatus.Pending);

            return Json(new { success = true, count = totalCount, comments = pendingComments });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid commentId, Guid movieId)
        {
            await _mediator.Send(new UpdateCommentStatusCommand { CommentId = commentId, NewStatus = CommentStatus.Approved });
            return Json(new { success = true, message = "Yorum başarıyla onaylandı." });
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid commentId, Guid movieId)
        {
            var result = await _mediator.Send(new UpdateCommentStatusCommand { CommentId = commentId, NewStatus = CommentStatus.Rejected });

            if (!result.IsSuccess)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, message = "Yorum reddedildi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid commentId, Guid movieId)
        {
            var result = await _mediator.Send(new DeleteCommentCommand
            {
                CommentDto = new DeleteCommentDto { CommentId = commentId, MovieId = movieId }
            });

            
            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, message = "Yorum kalıcı olarak silindi." });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction([FromForm] List<Guid> selectedComments, [FromForm] List<Guid> selectedMovieIds, [FromForm] string action)
        {
            if (selectedComments == null || !selectedComments.Any())
                return Json(new { success = false, message = "Hiçbir yorum seçilmedi." });

            for (int i = 0; i < selectedComments.Count; i++)
            {
                var cid = selectedComments[i];
                var mid = selectedMovieIds.Count > i ? selectedMovieIds[i] : Guid.Empty;

                if (action == "approve")
                    await _mediator.Send(new UpdateCommentStatusCommand { CommentId = cid, NewStatus = CommentStatus.Approved });
                else if (action == "reject")
                    await _mediator.Send(new UpdateCommentStatusCommand { CommentId = cid, NewStatus = CommentStatus.Rejected });
                else if (action == "delete")
                    await _mediator.Send(new DeleteCommentCommand { CommentDto = new DeleteCommentDto { CommentId = cid, MovieId = mid } });
            }

            return Json(new { success = true, message = $"Seçili yorumlar için '{action}' işlemi tamamlandı.", count = selectedComments.Count });
        }
    }
}