using MovieMvcProject.Domain.Enums;

namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    

    public class CommentDtoResponse
    {
        public Guid CommentId { get; set; }
        public required string Content { get; set; }

        public Guid MovieId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? MovieReview { get; set; }
        public string MovieTitle { get; set; } = string.Empty;

        public string? UserProfileImageUrl { get; set; }
        public required string UserId { get; set; }
        public required string Username { get; set; }

        public Guid? ParentId { get; set; }

        public List<CommentDtoResponse> Replies { get; set; } = new();

        // Yorum Moderasyon durumu
        public CommentStatus Status { get; set; }

        //  Okunabilir status adı (View'da kullanmak için)
        public string StatusDisplay => Status switch
        {
            CommentStatus.Pending => "Beklemede",
            CommentStatus.Approved => "Onaylandı",
            CommentStatus.Rejected => "Reddedildi",
            _ => "Bilinmiyor"
        };

        

        public string StatusBadgeClass => Status switch
        {
            CommentStatus.Approved => "bg-green-500/20 text-green-400",
            CommentStatus.Pending => "bg-yellow-500/20 text-yellow-400",
            CommentStatus.Rejected => "bg-red-500/20 text-red-400",
            _ => "bg-gray-500/20 text-gray-400"
        };

    }

}
