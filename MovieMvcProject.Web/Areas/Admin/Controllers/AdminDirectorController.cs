using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieMvcProject.Application.Commons;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.DTOs.ResponseDto;
using MovieMvcProject.Application.Features.Directors.Commands;
using MovieMvcProject.Application.Features.Directors.Queries;
using MovieMvcProject.Web.Areas.Admin.Models;
using MovieMvcProject.Web.Models;

namespace MovieMvcProject.Web.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    public class AdminDirectorController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public AdminDirectorController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAllDirectorsQuery(pageNumber, pageSize, searchTerm));
            var vm = new PagedResult<DirectorLookupViewModel>(
                _mapper.Map<List<DirectorLookupViewModel>>(result.Items),
                result.TotalCount, result.PageNumber, result.PageSize);
            return View(vm);
        }




        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateDirectorViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDirectorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = _mapper.Map<CreateDirectorDto>(model);
            var command = new CreateDirectorCommand(dto);
            var directorId = await _mediator.Send(command);

            TempData["Success"] = "Yönetmen başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _mediator.Send(new GetDirectorForEditQuery(id));
            if (dto == null) return NotFound();
            var vm = _mapper.Map<DirectorEditViewModel>(dto);
            return View(vm);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DirectorEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var fresh = await _mediator.Send(new GetDirectorForEditQuery(model.DirectorId));
                if (fresh != null)
                    model.Movies = _mapper.Map<List<DirectorMovieItemViewModel>>(fresh.Movies?.Items ?? new List<MovieDtoResponse>());
                return View(model);
            }
            var command = _mapper.Map<UpdateDirectorCommand>(model);
            var success = await _mediator.Send(command);
            if (success)
            {
                TempData["Success"] = "Yönetmen başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Güncelleme başarısız oldu.");
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteDirectorCommand(id));
            TempData["Success"] = "Yönetmen silindi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMovie(Guid directorId, Guid movieId)
        {
            var success = await _mediator.Send(new RemoveMovieFromDirectorCommand(directorId, movieId));
            if (success)
            {
                TempData["Success"] = "Film yönetmenden kaldırıldı.";
            }
            else
            {
                TempData["Error"] = "İşlem başarısız oldu.";
            }
            return RedirectToAction("Edit", new { id = directorId });
        }
    }
}
