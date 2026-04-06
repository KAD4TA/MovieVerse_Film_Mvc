namespace MovieMvcProject.Application.DTOs.Search
{
    

    public record DirectorSearchDocument(
        Guid DirectorId = default,             
        string Name = "",                      
        string? PhotoUrl = null,               
        string ProfilePath = "",               
        int? Height = null,                    
        DateTime? BirthDate = null,            
        string? BirthPlace = null              
    );
}
