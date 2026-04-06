
using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Comments.Commands
{
    public class UpdateCommentCommand : IRequest<CommentDtoResponse>
    {
        public required UpdateCommentDto CommentDto { get; set; }
    }
}
