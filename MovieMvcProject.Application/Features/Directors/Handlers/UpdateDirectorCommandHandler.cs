
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Directors.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;

namespace MovieMvcProject.Application.Features.Directors.Handlers
{
    public class UpdateDirectorCommandHandler : IRequestHandler<UpdateDirectorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly IElasticSearchService _elastic;
        private readonly ILogger<UpdateDirectorCommandHandler> _logger;

        public UpdateDirectorCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cache,
            IElasticSearchService elastic,
            ILogger<UpdateDirectorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
            _elastic = elastic;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateDirectorCommand request, CancellationToken ct)
        {
            try
            {
                
                var director = await _unitOfWork.Directors.GetByIdAsync(request.DirectorId);
                if (director == null)
                    throw new NotFoundException("Yönetmen bulunamadı.");

                
                _mapper.Map(request, director);

                
                await _unitOfWork.SaveChangesAsync(ct);

               
                var reloadedDirector = await _unitOfWork.Directors.GetByIdWithMoviesAsync(request.DirectorId);
                if (reloadedDirector == null)
                    throw new InvalidOperationException("Yönetmen güncellendi ama tekrar yüklenemedi.");

                
                await InvalidateDirectorCaches(reloadedDirector.DirectorId, ct);

               
                var searchDoc = _mapper.Map<DirectorSearchDocument>(reloadedDirector);
                await _elastic.IndexAsync(searchDoc, "directors", reloadedDirector.DirectorId, ct);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yönetmen güncellenirken hata oluştu. DirectorId: {DirectorId}", request.DirectorId);
                throw;
            }
        }

        private async Task InvalidateDirectorCaches(Guid directorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("directors:list:*", ct);           
            await _cache.RemoveByPatternAsync($"directors:edit:{directorId}:*", ct);
            await _cache.RemoveByPatternAsync($"directors:detail:{directorId}:*", ct);
        }
    }
}