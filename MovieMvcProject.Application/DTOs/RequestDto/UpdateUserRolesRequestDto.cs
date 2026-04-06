namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class UpdateUserRolesRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> NewRoles { get; set; } = new List<string>();
    }
}
