using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MovieMvcProject.Application.Features.Actors.Queries;

namespace MovieMvcProject.WebMvc.Controllers;

[Authorize]
public class ActorsController : Controller
{
    private readonly IMediator _mediator;
    public ActorsController(IMediator mediator) => _mediator = mediator;

    

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, int pageNumber = 1)
    {
       
        var result = await _mediator.Send(new GetActorWithMoviesQuery(id, pageNumber, 10));
        return View(result);
    }

    
}