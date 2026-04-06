namespace MovieMvcProject.Web.Models
{
    public class DirectorLookupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public int MovieCount { get; set; }
    }
}
