


using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Features.Movies.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieMvcProject.Application.Features.Movies.Handlers
{
    public class UpdateMovieSliderStatusCommandHandler : IRequestHandler<UpdateMovieSliderStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateMovieSliderStatusCommandHandler> _logger;

        public UpdateMovieSliderStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<UpdateMovieSliderStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateMovieSliderStatusCommand request, CancellationToken cancellationToken)
        {
            // 1. Veritabanından filmi bulma
            var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);

            if (movie == null)
            {
                _logger.LogWarning("Movie not found for Slider Update: {MovieId}", request.MovieId);
                return false;
            }

            // 2. Durumu güncelleme
            movie.IsOnSlider = request.IsOnSlider;

            await _unitOfWork.Movies.UpdateAsync(movie);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;

            if (result)
            {
                try
                {
                    
                    await _cacheService.RemoveByPrefixAsync("movies:", cancellationToken);
                    _logger.LogInformation("Cache invalidated for movies after slider status update.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while invalidating cache for movies.");
                }
            }

            return result;
        }
    }
}