
using MediatR;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Comments.Commands
{
    public class CreateCommentCommand : IRequest<CommentDtoResponse>
    {
        public required CreateCommentDto CommentDto { get; set; }
        public required string UserId { get; set; } 
    }

}
