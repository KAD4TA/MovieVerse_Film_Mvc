using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

public class SearchCommentsQuery : IRequest<PagedResult<CommentDtoResponse>>
{
    public required string Query { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}