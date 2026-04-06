using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Features.Directors.Queries;
using MovieMvcProject.Web.Models;

namespace MovieMvcProject.WebMvc.Controllers;


[Authorize]
public class DirectorsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    public DirectorsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;

    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, int pageNumber = 1)
    {
        var query = new GetDirectorWithMoviesQuery(id, pageNumber, 10);
        var dto = await _mediator.Send(query);
        if (dto == null) return NotFound();

        var viewModel = _mapper.Map<DirectorDetailViewModel>(dto);
        return View(viewModel);
    }
}