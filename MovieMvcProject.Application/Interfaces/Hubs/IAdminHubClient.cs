namespace MovieMvcProject.Application.Interfaces.Hubs
{
    public interface IAdminHubClient
    {
        
        Task ReceiveNotification(string message, string title);
        Task UpdateDashboardStats(int newTotalUserCount);
    }
}
