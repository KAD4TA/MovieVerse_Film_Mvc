namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class UpdateMovieDto : CreateMovieDto
    {
        public required Guid MovieId { get; set; }
    }

}
