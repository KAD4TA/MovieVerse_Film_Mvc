
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.Features.Comments.Commands;
using MovieMvcProject.Application.Features.Comments.Queries;
using System.Security.Claims;

namespace MovieMvcProject.Web.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        
        public async Task<IActionResult> List(Guid movieId, int pageNumber = 1)
        {
            var query = new GetCommentsByMovieQuery
            {
                MovieId = movieId,
                PageNumber = pageNumber,
                PageSize = 10,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                IsAdmin = User.IsInRole("Admin")
            };

            var result = await _mediator.Send(query);

            //  QueryHandler'dan gelen flat listeyi ağaç yapısına çeviriyoruz
            if (result?.Items != null)
            {
                
                result.Items = CommentTreeBuilder.BuildTree(result.Items.ToList());
            }

            
            return PartialView("_CommentsListPartial", result);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(DeleteCommentDto commentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            var comment = await _mediator.Send(new GetCommentByIdQuery { CommentId = commentDto.CommentId });

            if (comment == null) return Json(new { success = false, message = "Yorum bulunamadı." });

            // Sadece sahibi veya Admin silebilir
            if (comment.UserId != userId && !User.IsInRole("admin"))
                return Json(new { success = false, message = "Bu işlem için yetkiniz yok." });

            var response = await _mediator.Send(new DeleteCommentCommand { CommentDto = commentDto });
            return Json(new { success = response.Success, message = response.Message });
        }



        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CreateCommentDto commentDto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });

            try
            {
                await _mediator.Send(new CreateCommentCommand
                {
                    CommentDto = commentDto,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ""
                });

                
                return Json(new
                {
                    success = true,
                    message = "Yorumunuz alındı, onay sonrası herkese görünecektir. Şu an sadece siz görebilirsiniz."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Beklenmedik bir hata oluştu: " + ex.Message });
            }
        }

        

        [HttpGet]
        public async Task<IActionResult> Edit(Guid commentId)
        {
            var comment = await _mediator.Send(new GetCommentByIdQuery { CommentId = commentId });

            
            if (comment == null || comment.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            var dto = new UpdateCommentDto
            {
                CommentId = comment.CommentId,
                MovieId = comment.MovieId,
                Content = comment.Content,
                MovieReview = comment.MovieReview
            };

            return PartialView("_EditCommentPartial", dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCommentDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _mediator.Send(new GetCommentByIdQuery { CommentId = updateDto.CommentId });

            if (existing == null || existing.UserId != currentUserId)
                return Json(new { success = false, message = "Yetkisiz işlem veya yorum bulunamadı." });

            await _mediator.Send(new UpdateCommentCommand { CommentDto = updateDto });

            return Json(new { success = true, message = "Yorumunuz güncellendi ve tekrar onaya gönderildi." });
        }




    }
}
