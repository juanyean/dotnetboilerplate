using Microsoft.AspNetCore.SignalR;
using MyDotNetApp.Application.Interfaces;

namespace MyDotNetApp.Infrastructure.Hubs;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        => _hubContext = hubContext;

    public Task BroadcastAsync(string message, CancellationToken ct = default)
        => _hubContext.Clients.All.SendAsync("ReceiveNotification", message, ct);

    public Task SendToUserAsync(string userId, string message, CancellationToken ct = default)
        => _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message, ct);
}
