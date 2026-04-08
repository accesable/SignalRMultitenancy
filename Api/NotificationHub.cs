using Microsoft.AspNetCore.SignalR;

namespace Api;

public class NotificationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var resourceId = httpContext!.Request.Query["resourceId"];
        var userId = httpContext!.Request.Query["userId"];
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