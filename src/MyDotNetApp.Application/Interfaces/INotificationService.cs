namespace MyDotNetApp.Application.Interfaces;

public interface INotificationService
{
    Task BroadcastAsync(string message, CancellationToken ct = default);
    Task SendToUserAsync(string userId, string message, CancellationToken ct = default);
}
