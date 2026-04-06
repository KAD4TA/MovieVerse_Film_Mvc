using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class ManageRolesViewModel
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }

        public required string Email { get; set; }

        public required string UserPhoto { get; set; }
        public Gender? Gender { get; set; }
        public DateTime CreatedAt { get; set; }

      
        public DateTime? BirthDate { get; set; }

        
        public List<RoleViewModel> AllRoles { get; set; } = new();

       
        public IList<string> UserRoles { get; set; } = new List<string>();
    }
}
