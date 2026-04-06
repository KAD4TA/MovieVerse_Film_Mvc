using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieMvcProject.Application.DTOs.RequestDto;
using MovieMvcProject.Application.Interfaces;
using MovieMvcProject.Domain.Identity;
using MovieMvcProject.Web.Areas.Admin.Models;

namespace MovieMvcProject.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")] 
    public class AdminRoleController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;

        public AdminRoleController(
            IUserService userService,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IMapper mapper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }



        [HttpGet]
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("UserList", "Admin", new { area = "Admin" });
            }

            
            var model = _mapper.Map<ManageRolesViewModel>(user);

            
            var userRoles = await _userManager.GetRolesAsync(user);
            model.UserRoles = userRoles.ToList();

            var allRoles = await _roleManager.Roles.ToListAsync();
            model.AllRoles = allRoles.Select(r => new RoleViewModel
            {
                RoleId = r.Id,
                RoleName = r.Name ?? "",
                IsSelected = userRoles.Contains(r.Name ?? "")
            }).ToList();

            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                
                return RedirectToAction("UserList", "Admin", new { area = "Admin" });
            }

            var selectedRoleNames = model.AllRoles
                .Where(r => r.IsSelected)
                .Select(r => r.RoleName)
                .ToList();

            var dto = new UpdateUserRolesRequestDto
            {
                UserId = model.UserId,
                NewRoles = selectedRoleNames
            };

            var result = await _userService.UpdateUserRoles(dto);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Roller başarıyla güncellendi.";
            }
            else
            {
                TempData["Error"] = result.Message ?? "Rol güncelleme başarısız.";
            }

          
            return RedirectToAction("UserList", "Admin", new { area = "Admin" });
        }
    }
}
