
using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Application.Features.Comments.Commands
{
    public class UpdateCommentStatusCommand : IRequest<BaseResponse>
    {
        public Guid CommentId { get; set; }
        public CommentStatus NewStatus { get; set; }
    }
}