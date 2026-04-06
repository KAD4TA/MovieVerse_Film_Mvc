
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Application.Features.Comments.Queries
{
    public class GetAllCommentsQuery : IRequest<PagedResult<CommentDtoResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public CommentStatus? Status { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
    }
}