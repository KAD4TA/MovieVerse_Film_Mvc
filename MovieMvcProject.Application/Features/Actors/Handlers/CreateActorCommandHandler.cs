


using AutoMapper;
using MediatR;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Actors.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Application.Features.Actors.Handlers
{
    public class CreateActorCommandHandler : IRequestHandler<CreateActorCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public CreateActorCommandHandler(
            IUnitOfWork unitOfWork,
            IElasticSearchService elasticSearchService,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _elasticSearchService = elasticSearchService;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Guid> Handle(CreateActorCommand request, CancellationToken ct)
        {
            if (request.Dto == null)
                throw new ArgumentNullException(nameof(request.Dto));

            var actor = _mapper.Map<Actor>(request.Dto);
            actor.ActorId = Guid.NewGuid();

            await _unitOfWork.Actors.AddAsync(actor);
            await _unitOfWork.SaveChangesAsync(ct);

            // Elasticsearch Indexing
            var searchDoc = _mapper.Map<ActorSearchDocument>(actor);
            await _elasticSearchService.IndexAsync(searchDoc, "actors", actor.ActorId, ct);

            // Cache temizleme
            await InvalidateActorCaches(actor.ActorId, ct);

            return actor.ActorId;
        }

        private async Task InvalidateActorCaches(Guid actorId, CancellationToken ct)
        {
            await _cacheService.RemoveByPatternAsync("actors:list:*", ct);
            await _cacheService.RemoveByPatternAsync($"actors:edit:{actorId}:*", ct);
            await _cacheService.RemoveByPatternAsync($"actors:detail:{actorId}:*", ct);
        }
    }
}