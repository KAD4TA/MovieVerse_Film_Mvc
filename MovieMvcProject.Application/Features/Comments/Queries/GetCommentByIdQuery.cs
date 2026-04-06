
using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Comments.Queries
{
    public class GetCommentByIdQuery : IRequest<CommentDtoResponse>
    {
        public Guid CommentId { get; set; }
    }




}
