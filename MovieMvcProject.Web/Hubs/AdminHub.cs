using Microsoft.AspNetCore.SignalR;
using MovieMvcProject.Application.Interfaces.Hubs;

namespace MovieMvcProject.Web.Hubs
{
    public class AdminHub : Hub<IAdminHubClient>
    {
        
        public override async Task OnConnectedAsync()
        {
            if (Context.User.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            await base.OnConnectedAsync();
        }
    }
}
