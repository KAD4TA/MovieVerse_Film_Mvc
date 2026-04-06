using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Movies.Queries;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Movies.Handlers
{
    public class GetMovieForUpdateQueryHandler : IRequestHandler<GetMovieForUpdateQuery, MovieUpdateViewDto>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<ValidationResource> _localizer;

        public GetMovieForUpdateQueryHandler(
            IMovieRepository movieRepository,
            IMapper mapper,
            IStringLocalizer<ValidationResource> localizer)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<MovieUpdateViewDto> Handle(GetMovieForUpdateQuery request, CancellationToken cancellationToken)
        {
            // Repository'den tüm ilişkileriyle (Translations, Director, Actors) çekme
            var movieEntity = await _movieRepository.GetByIdAsync(request.MovieId);

            if (movieEntity == null)
            {
                throw new NotFoundException("Movie", request.MovieId, _localizer);
            }

           
            return _mapper.Map<MovieUpdateViewDto>(movieEntity);
        }
    }
}