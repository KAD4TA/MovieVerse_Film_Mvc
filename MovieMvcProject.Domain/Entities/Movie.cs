using MovieMvcProject.Domain.Entities;
using MovieMvcProject.Domain.Entities.EntityTranslations;
using MovieMvcProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Movie
{
    [Key]
    public Guid MovieId { get; set; } = Guid.NewGuid();

    public required int Year { get; set; }
    public required double Rating { get; set; }
    public Category Category { get; set; }

    
    public int DurationInMinutes { get; set; }

    public string? PosterUrl { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public bool IsOnSlider { get; set; } = false;

    
    public Guid? DirectorId { get; set; }
    public required Director Director { get; set; } = null!;

    // --- NAVİGASYON KOLEKSİYONLARI ---
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public double MovieAvgReviewRate { get; set; } = 0;

    public List<MovieTranslation> Translations { get; set; } = new List<MovieTranslation>();

    //--- ELASTICSEARCH/NOTMAPPED ALANLARI ---
    [NotMapped] public string TitleTr { get; set; } = string.Empty;
    [NotMapped] public string TitleEn { get; set; } = string.Empty;
    [NotMapped] public string DescriptionTr { get; set; } = string.Empty;
    [NotMapped] public string DescriptionEn { get; set; } = string.Empty;


}