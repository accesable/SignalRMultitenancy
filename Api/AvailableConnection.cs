using System.Collections.Concurrent;

namespace Api;

public static class AvailableConnection
{
    private static readonly ConcurrentDictionary<string, string> Connections = new(); // UserId - ConnectionId;

    public static void AddConnection(string userId, string connectionId)
    {
        Connections.AddOrUpdate(userId, connectionId, (userId, connectionId) =>
        {
            return connectionId;
        });
    }

    public static string GetConnection(string userId)
    {
        Connections.TryGetValue(userId, out string? connectionId);

        return connectionId ?? string.Empty;
    }

    public static void RemoveConnection(string userId)
    {
        Connections.Remove(userId, out _);
    }

    public static IEnumerable<string> GetAllUserIds()
    {
        return Connections.Keys;
    }
}