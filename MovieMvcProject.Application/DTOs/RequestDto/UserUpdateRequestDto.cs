namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class UserUpdateRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

       
    }
}
