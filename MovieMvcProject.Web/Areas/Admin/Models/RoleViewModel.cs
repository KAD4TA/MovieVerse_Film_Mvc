namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class RoleViewModel
    {
        public required string RoleId { get; set; }
        public required string RoleName { get; set; }
        public bool IsSelected { get; set; } 
    }
}
