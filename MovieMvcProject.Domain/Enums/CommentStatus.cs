
namespace MovieMvcProject.Domain.Enums
{
    public enum CommentStatus
    {
        Pending = 0,    // Beklemede (Yeni yorum, henüz incelenmedi)
        Approved = 1,   // Onaylandı (Yayınlandı)
        Rejected = 2    // Reddedildi (Spam, uygunsuz vs.)
    }
}