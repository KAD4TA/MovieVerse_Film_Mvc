using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.DTOs.LiveSearch;
using MovieMvcProject.Application.Features.LiveSearch.Queries;
using MovieMvcProject.Application.Interfaces;

namespace MovieMvcProject.Application.Features.LiveSearch.Handlers
{
    public class GetUserSearchQueryHandler : IRequestHandler<GetUserSearchQuery, List<LiveSearchResultDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserSearchQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<LiveSearchResultDto>> Handle(GetUserSearchQuery request, CancellationToken cancellationToken)
        {
            var query = request.Query;

            var users = await _context.Users
                
                .Where(u => u.FullName.Contains(query) || u.UserName.Contains(query) || u.Email.Contains(query) || u.Id.Contains(query))
                .Take(request.PageSize)
                .Select(u => new LiveSearchResultDto(
                     u.Id, 
                     u.FullName + " (" + u.UserName + ")", 
                     "Kullanıcı",
                     "/Admin/Admin/EditUser/" + u.Id,
                     u.ProfileImageUrl
                ))
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
