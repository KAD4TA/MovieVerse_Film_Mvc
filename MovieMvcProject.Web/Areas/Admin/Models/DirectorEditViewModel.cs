namespace MovieMvcProject.Web.Areas.Admin.Models
{
    public class DirectorEditViewModel
    {
        public Guid DirectorId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? BirthPlace { get; set; }

        public int? Height { get; set; }

        
        public List<DirectorMovieItemViewModel> Movies { get; set; } = new();
    }
}
