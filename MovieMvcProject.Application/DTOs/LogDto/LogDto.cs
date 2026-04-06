namespace MovieMvcProject.Application.DTOs.LogDto
{
    public class LogDto
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? Level { get; set; } // Bilgi, Hata, Uyarı vb.
        public string? Message { get; set; }
        public string? Exception { get; set; } // Hata detayları için
        public string? Properties { get; set; } // JSONB verisi
    }
}
