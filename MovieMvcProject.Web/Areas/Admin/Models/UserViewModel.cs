namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class UserViewModel
    {
        public required string UserId { get; set; }

        public required string UserPhoto { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }

        public bool IsActive { get; set; }
        public required string RoleNames { get; set; } // Örn: "Admin, User"
    }
}
