using MovieMvcProject.Domain.Resources;
using System.ComponentModel.DataAnnotations;
namespace MovieMvcProject.Domain.Enums
{
    public enum Category
    {
        [Display(ResourceType = typeof(EnumResource), Name = "Category_Action")]
        Action,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Comedy")]
        Comedy,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Drama")]
        Drama,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Horror")]
        Horror,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_SciFi")]
        SciFi,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Anime")]
        Anime,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Thriller")]
        Thriller,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Romance")]
        Romance,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Fantasy")]
        Fantasy,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Adventure")]
        Adventure,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Crime")]
        Crime,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Documentary")]
        Documentary,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Animation")]
        Animation,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Musical")]
        Musical,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Western")]
        Western,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_Biography")]
        Biography,

        [Display(ResourceType = typeof(EnumResource), Name = "Category_War")]
        War
    }
}

