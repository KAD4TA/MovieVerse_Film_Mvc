namespace MovieMvcProject.Web.Models
{
    public class ActorListViewModel
    {
        public Guid? ActorId { get; set; }


        public string Name { get; set; }


        public string? AvatarUrl { get; set; }

        public DateTime? BirthDate { get; set; }



        public int MovieCount { get; set; }
    }
}
