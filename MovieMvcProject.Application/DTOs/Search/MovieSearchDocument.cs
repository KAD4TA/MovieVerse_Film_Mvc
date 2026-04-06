namespace MovieMvcProject.Application.DTOs.Search
{
    public record MovieSearchDocument(
    Guid Id,
    string TitleTr,
    string TitleEn,
    string DescriptionTr,
    string DescriptionEn,
    string Category,
    double Rating,
    string PosterPath,
    int ReleaseYear);
}
