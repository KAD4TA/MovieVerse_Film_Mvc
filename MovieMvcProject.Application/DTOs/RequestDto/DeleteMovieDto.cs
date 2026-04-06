namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class DeleteMovieDto
    {
        public required Guid MovieId { get; set; }   // Silinecek film Id'si
    }

}
