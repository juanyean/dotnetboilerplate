using Microsoft.AspNetCore.SignalR;

namespace MyDotNetApp.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    public async Task BroadcastMessage(string message)
        => await Clients.All.SendAsync("ReceiveNotification", message);
}
