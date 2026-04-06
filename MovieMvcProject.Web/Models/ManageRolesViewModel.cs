namespace MovieMvcProject.Web.Models
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; // YENİ

        public List<RoleViewModel> AllRoles { get; set; } = new List<RoleViewModel>();
        public List<string> UserRoles { get; set; } = new List<string>();
    }
}