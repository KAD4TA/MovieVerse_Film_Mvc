using MediatR;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Categories.Queries;
using MovieMvcProject.Application.Interfaces.IRepositories;

namespace MovieMvcProject.Application.Features.Categories.Handlers
{
    public class GetMoviesByCategoryHandler : IRequestHandler<GetMoviesByCategoryQuery, PagedResult<MovieDtoResponse>>
    {
        private readonly IMovieRepository _movieRepository;

        public GetMoviesByCategoryHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<PagedResult<MovieDtoResponse>> Handle(GetMoviesByCategoryQuery request, CancellationToken cancellationToken)
        {
           
            var pagedMovies = await _movieRepository.GetByCategoryAsync(
                request.CategoryName,
                request.LanguageCode,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Mapping işlemi
            var dtos = pagedMovies.Items.Select(m => new MovieDtoResponse
            {
                MovieId = m.MovieId,
                Title = m.Translations.FirstOrDefault(t => t.LanguageCode == request.LanguageCode)?.Title
                        ?? m.Translations.FirstOrDefault()?.Title ?? "N/A",
                Year = m.Year,
                Rating = m.Rating,
                Category = m.Category.ToString(),
                PosterUrl = m.PosterUrl
            }).ToList();

            return new PagedResult<MovieDtoResponse>(dtos, pagedMovies.TotalCount, request.PageNumber, request.PageSize);
        }
    }
}