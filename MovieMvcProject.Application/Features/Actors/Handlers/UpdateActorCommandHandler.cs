

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;

namespace MovieMvcProject.Application.Features.Actors.Commands.UpdateActor
{
    public class UpdateActorCommandHandler : IRequestHandler<UpdateActorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly IElasticSearchService _elastic;
        private readonly ILogger<UpdateActorCommandHandler> _logger;

        public UpdateActorCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cache,
            IElasticSearchService elastic,
            ILogger<UpdateActorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
            _elastic = elastic;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateActorCommand request, CancellationToken ct)
        {
            try
            {
                
                var actor = await _unitOfWork.Actors.GetByIdAsync(request.ActorId);
                if (actor == null)
                    throw new NotFoundException("Aktör bulunamadı.");

                
                _mapper.Map(request, actor);  

               
                await _unitOfWork.SaveChangesAsync(ct);

                
                var reloadedActor = await _unitOfWork.Actors.GetActorWithMoviesAsync(request.ActorId); 
                if (reloadedActor == null)
                    throw new InvalidOperationException("Aktör güncellendi ama tekrar yüklenemedi.");

                
                await InvalidateActorCaches(reloadedActor.ActorId, ct);

                
                var searchDoc = _mapper.Map<ActorSearchDocument>(reloadedActor);
                await _elastic.IndexAsync(searchDoc, "actors", reloadedActor.ActorId, ct);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktör güncellenirken hata oluştu. ActorId: {ActorId}", request.ActorId);
                throw;
            }
        }

        private async Task InvalidateActorCaches(Guid actorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("actors:list:*", ct);           
            await _cache.RemoveByPatternAsync($"actors:edit:{actorId}:*", ct);
            await _cache.RemoveByPatternAsync($"actors:detail:{actorId}:*", ct);
        }
    }
}
