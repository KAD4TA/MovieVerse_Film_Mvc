namespace MovieMvcProject.Application.DTOs.LiveSearch
{
    public record LiveSearchResultDto(
        // Varlığın benzersiz kimliği (string formatında)
        string Id,

        // Görüntülenecek ana başlık
        string Title,

        // Sonucun tipini belirtir (Örn: "Film", "Kullanıcı", "Oyuncu")
        string Type,

        // Tıklanınca gidilecek Admin Controller URL'si
        string Url,

        string? PhotoUrl
    );
}
