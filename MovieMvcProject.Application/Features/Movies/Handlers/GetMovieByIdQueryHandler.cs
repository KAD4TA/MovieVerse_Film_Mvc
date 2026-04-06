

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Movies.Queries;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Resources;
using System.Globalization;

namespace MovieMvcProject.Application.Features.Movies.Handlers
{
    public class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, MovieDetailDto>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;
        private readonly IStringLocalizer<ValidationResource> _localizer;

        public GetMovieByIdQueryHandler(
            IMovieRepository movieRepository,
            ICacheService cacheService,
            IMapper mapper,
            IApplicationDbContext context,
            IStringLocalizer<ValidationResource> localizer)
        {
            _movieRepository = movieRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _context = context;
            _localizer = localizer;
        }

        
        public async Task<MovieDetailDto> Handle(
    GetMovieByIdQuery request,
    CancellationToken cancellationToken)
        {
            var currentLang =
                CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName
                .ToLower();

            var cacheKey =
                $"movie:detail:{request.MovieId}:{currentLang}";

            var cached =
                await _cacheService.GetAsync<MovieDetailDto>(
                    cacheKey,
                    cancellationToken);

            if (cached != null)
                return cached;

            var movieEntity =
                await _movieRepository.GetByIdAsync(
                    request.MovieId);

            if (movieEntity == null)
                throw new NotFoundException(
                    nameof(movieEntity),
                    request.MovieId,
                    _localizer);

            //  Film mapleme
            var resultDto =
                _mapper.Map<MovieDetailDto>(
                    movieEntity,
                    opt =>
                    {
                        opt.Items["LanguageCode"] =
                            currentLang;
                    });

            //  Nested yorumları çekme
            var flatComments =
                await _context.Comments
                .Where(c => c.MovieId == request.MovieId)
                .ProjectTo<CommentDtoResponse>(
                    _mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            resultDto.Comments =
                CommentTreeBuilder.BuildTree(flatComments);

            await _cacheService.SetAsync(
                cacheKey,
                resultDto,
                TimeSpan.FromMinutes(30),
                cancellationToken);

            return resultDto;
        }

    }
}