
using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Comments.Commands
{
    public class DeleteCommentCommand : IRequest<DeleteCommentDtoResponse>
    {
        public required DeleteCommentDto CommentDto { get; set; }
    }
}
