namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class DirectorLookupDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string PhotoUrl { get; set; } = "profile-images/default-profile.png";

        public DateTime? BirthDate { get; set; }

        public int MovieCount { get; set; } = 0;
    }
}
