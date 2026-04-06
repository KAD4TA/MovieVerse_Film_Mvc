namespace MovieMvcProject.Application.Interfaces.Notification
{
    public interface INotificationService
    {
        
        Task NotifyAdminAsync(string message, string title = "Sistem Bildirimi");
    }
}
