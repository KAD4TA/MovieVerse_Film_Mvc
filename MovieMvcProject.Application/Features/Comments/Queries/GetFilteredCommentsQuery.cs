
using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Comments.Queries
{
    public class GetFilteredCommentsQuery : IRequest<PagedResult<CommentDtoResponse>>
    {
        public Guid? MovieId { get; set; }          // Belirli bir filme ait yorumlar
        public string? UserId { get; set; }           // Belirli bir kullanıcıya ait yorumlar
        public string? Keyword { get; set; }        // İçerikte arama
        public string? SortBy { get; set; }         // Tarih, puan vs.
        public bool Descending { get; set; } = true;

        // 📄 Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
