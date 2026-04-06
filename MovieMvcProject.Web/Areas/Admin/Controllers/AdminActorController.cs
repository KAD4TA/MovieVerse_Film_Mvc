
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.Features.Actors.Commands;
using MovieMvcProject.Application.Features.Actors.Queries;
using MovieMvcProject.Web.Areas.Admin.Models;
using MovieMvcProject.Web.Models;

namespace MovieMvcProject.Web.Areas.Admin.Controllers
{
    

    [Area("Admin")]
    public class AdminActorController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public AdminActorController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAllActorsQuery(pageNumber, pageSize, searchTerm));
            var vm = new PagedResult<ActorListViewModel>(
                _mapper.Map<List<ActorListViewModel>>(result.Items),
                result.TotalCount, result.PageNumber, result.PageSize);
            return View(vm);
        }



        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateActorViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateActorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = _mapper.Map<CreateActorDto>(model);
            var command = new CreateActorCommand(dto);
            var actorId = await _mediator.Send(command);

            TempData["Success"] = "Oyuncu başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _mediator.Send(new GetActorForEditQuery(id));
            if (dto == null) return NotFound();
            return View(_mapper.Map<ActorEditViewModel>(dto));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ActorEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var fresh = await _mediator.Send(new GetActorForEditQuery(model.ActorId));
                if (fresh != null)
                    model.Movies = _mapper.Map<List<ActorMovieItemViewModel>>(fresh.Movies ?? new());
                return View(model);
            }

            var command = _mapper.Map<UpdateActorCommand>(model);
            var success = await _mediator.Send(command);

            if (success)
            {
                TempData["Success"] = "Oyuncu güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Güncelleme başarısız.");
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteActorCommand(id));
            TempData["Success"] = "Oyuncu silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMovie(Guid actorId, Guid movieId)
        {
            await _mediator.Send(new RemoveMovieFromActorCommand(actorId, movieId));
            TempData["Success"] = "Film kaldırıldı.";
            return RedirectToAction(nameof(Edit), new { id = actorId });
        }
    }
}
