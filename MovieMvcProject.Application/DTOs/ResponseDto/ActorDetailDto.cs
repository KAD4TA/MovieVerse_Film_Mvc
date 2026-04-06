using MovieMvcProject.Application.Commons;

namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class ActorDetailDto
    {
        // AutoMapper için gerekli boş constructor
        public ActorDetailDto() { }

        
        public ActorDetailDto(Guid id, string name, string avatarUrl, int? height, string? birthplace, DateTime? birtdate, PagedResult<MovieDtoResponse> movies)
        {
            Id = id;
            Name = name;
            AvatarUrl = avatarUrl;
            Movies = movies;
            Height = height;
            BirthPlace = birthplace;
            BirthDate = birtdate;

        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }

        public int? Height { get; set; }

        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }
        public PagedResult<MovieDtoResponse> Movies { get; set; }
    }
}