using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Features.Directors.Queries
{
    public record GetDirectorForEditQuery(Guid DirectorId) : IRequest<DirectorDetailDto?>;
}
