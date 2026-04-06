namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class ActorListDto
    {
        public Guid ActorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }
        public int MovieCount { get; set; }
    }
}
