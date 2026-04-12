using Microsoft.AspNetCore.SignalR;

namespace Api;

public class NotificationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var resourceId = Context.User?.FindFirst("resourceId")?.Value;
        var userId = Context.User?.FindFirst("userId")?.Value;

        var connectionId = Context.ConnectionId;

        Context.Items["resourceId"] = resourceId;
        Context.Items["userId"] = userId;

        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        AvailableConnection.AddConnection(userId!, connectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        AvailableConnection.RemoveConnection(Context.Items["userId"]!.ToString()!);
        return base.OnDisconnectedAsync(exception);
    }
}