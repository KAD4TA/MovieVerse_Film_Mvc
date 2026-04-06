using MovieMvcProject.Application.Commons;

namespace MovieMvcProject.Application.DTOs.ResponseDto
{
   

    public class DirectorDetailDto
    {
        public DirectorDetailDto() { }

        public DirectorDetailDto(Guid id, string name, string photoUrl, DateTime? birthDate, string? birthPlace, int? height, PagedResult<MovieDtoResponse> movies)
        {
            Id = id;
            Name = name;
            PhotoUrl = photoUrl;
            BirthDate = birthDate;
            BirthPlace = birthPlace;
            Height = height;
            Movies = movies;
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public int? Height { get; set; }
        public PagedResult<MovieDtoResponse> Movies { get; set; } = PagedResult<MovieDtoResponse>.Empty(1, 10);
    }
}
