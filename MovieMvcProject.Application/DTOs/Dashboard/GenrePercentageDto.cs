namespace MovieMvcProject.Application.DTOs.Dashboard
{
    public class GenrePercentageDto
    {
        public string Name { get; set; } // Tür Adı (Örn: "Action", "Drama")
        public int Count { get; set; }  // O türe ait film sayısı
    }
}
