using Microsoft.AspNetCore.SignalR;
using MovieMvcProject.Application.Interfaces.Hubs;
using MovieMvcProject.Application.Interfaces.Notification;
using MovieMvcProject.Web.Hubs;

namespace MovieMvcProject.Infrastructure.Services.Notification
{
    public class NotificationService : INotificationService
    {
        
        private readonly IHubContext<AdminHub, IAdminHubClient> _hubContext;

        public NotificationService(IHubContext<AdminHub, IAdminHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAdminAsync(string message, string title)
        {
            
            await _hubContext.Clients.Group("Admins").ReceiveNotification(message, title);
        }

       
    }
}
