using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Comments.Queries
{
   
    public class GetCommentsByMovieQuery : IRequest<PagedResult<CommentDtoResponse>>
    {
        public required Guid MovieId { get; set; }
        public string? UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public bool IsAdmin { get; set; }
    }
}