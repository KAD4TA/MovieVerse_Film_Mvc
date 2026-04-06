using AutoMapper;
using MediatR;
using MovieMvcProject.Application.DTOs.Search;
using MovieMvcProject.Application.Features.Directors.Commands;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Application.Interfaces.Caching;
using MovieMvcProject.Application.Interfaces.Indexing;
using MovieMvcProject.Domain.Entities;

namespace MovieMvcProject.Application.Features.Directors.Handlers
{
    public class CreateDirectorCommandHandler : IRequestHandler<CreateDirectorCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticSearchService _elastic;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public CreateDirectorCommandHandler(IUnitOfWork unitOfWork, IElasticSearchService elastic, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _elastic = elastic;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<Guid> Handle(CreateDirectorCommand request, CancellationToken ct)
        {
            var director = _mapper.Map<Director>(request.Dto);
            director.DirectorId = Guid.NewGuid();

            await _unitOfWork.Directors.AddAsync(director);
            await _unitOfWork.SaveChangesAsync(ct);

            var searchDoc = _mapper.Map<DirectorSearchDocument>(director);
            await _elastic.IndexAsync(searchDoc, "directors", director.DirectorId, ct);

            await InvalidateDirectorCaches(director.DirectorId, ct);
            return director.DirectorId;
        }

        private async Task InvalidateDirectorCaches(Guid directorId, CancellationToken ct)
        {
            await _cache.RemoveByPatternAsync("directors:list:*", ct);
            await _cache.RemoveByPatternAsync($"directors:edit:{directorId}:*", ct);   
            await _cache.RemoveByPatternAsync($"directors:detail:{directorId}:*", ct);
        }
    }
}
