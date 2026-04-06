

namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class UserResponseDto
    {
        public required string Id { get; set; }

        
        public string? ProfileImageUrl { get; set; }

        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }

        
        public DateTime? BirthDate { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }

        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

}
