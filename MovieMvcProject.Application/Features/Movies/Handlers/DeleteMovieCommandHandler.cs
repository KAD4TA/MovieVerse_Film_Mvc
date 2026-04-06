



using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Movies.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Application.Interfaces.IRepositories;
using MovieMvcProject.Domain.Resources;

namespace MovieMvcProject.Application.Features.Movies.Handlers
{
    public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, DeleteMovieDtoResponse>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IStringLocalizer<ExceptionResource> _localizer;
        private readonly ILogger<DeleteMovieCommandHandler> _logger;

        public DeleteMovieCommandHandler(
            IMovieRepository movieRepository,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IElasticSearchService elasticSearchService,
            IStringLocalizer<ExceptionResource> localizer,
            ILogger<DeleteMovieCommandHandler> logger)
        {
            _movieRepository = movieRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _elasticSearchService = elasticSearchService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<DeleteMovieDtoResponse> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
        {
            var movieId = request.MovieDto.MovieId;

            _logger.LogInformation("Film silme işlemi başlatıldı: ID = {MovieId}", movieId);

            // 1. Filmin varlığını kontrol etme
            var movieToDelete = await _movieRepository.GetByIdAsync(movieId);
            if (movieToDelete == null)
            {
                _logger.LogWarning("Film bulunamadı: ID = {MovieId}", movieId);
                throw new NotFoundException(_localizer["MovieNotFound"] ?? "Film bulunamadı.");
            }

            // 2. Filmi Silme
            bool success = await _movieRepository.DeleteAsync(movieId);
            var dbResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (success && dbResult > 0)
            {
                _logger.LogInformation("Film DB'den silindi: ID = {MovieId}", movieId);

                // 3. Elasticsearch'ten Silme
                try
                {
                    await _elasticSearchService.DeleteAsync("movies", movieId.ToString(), cancellationToken);
                    _logger.LogInformation("Elasticsearch'ten film kaldırıldı: ID = {MovieId}", movieId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Elasticsearch silme hatası: ID = {MovieId}", movieId);
                }

               
                try
                {
                    
                    await _cacheService.RemoveByPatternAsync("movies:all:*", cancellationToken);

                    
                    await _cacheService.RemoveByPatternAsync($"movie:detail:{request.MovieDto.MovieId}:*", cancellationToken);

                   
                    await _cacheService.RemoveByPatternAsync("movies:search:*", cancellationToken);

                    _logger.LogInformation("Tüm ilgili cache'ler temizlendi. MovieId: {MovieId}", movieId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cache temizleme hatası: ID = {MovieId}", movieId);
                }

                return new DeleteMovieDtoResponse
                {
                    Success = true,
                    Message = _localizer["MovieDeletedSuccess"] ?? "Film başarıyla silindi."
                };
            }

            _logger.LogError("Film silme başarısız: ID = {MovieId}, Success = {Success}, DB Result = {DbResult}", movieId, success, dbResult);
            return new DeleteMovieDtoResponse
            {
                Success = false,
                Message = "Silme işlemi başarısız."
            };
        }
    }
}

