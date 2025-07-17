using System;
using MapGeneration;
using Sunrise.API.Visibility.Generation;

namespace Sunrise.API.Visibility;

public class VisibilityData
{
    static readonly Dictionary<Vector3Int, VisibilityData> Cache = new();

    VisibilityData(Room room)
    {
        TargetRoom = room ?? throw new ArgumentNullException(nameof(room));
        InitializeVisibilityData();
    }

    public Room TargetRoom { get; }
    public HashSet<Vector3Int> VisibleCoords { get; } = [];

    public bool IsVisible(Player player) => player is not { IsConnected: true } || IsVisible(Room.Get(player.Position));
    public bool IsVisible(Room? room) => room?.Identifier is not RoomIdentifier identifier || VisibleCoords.Contains(identifier.MainCoords);

    void InitializeVisibilityData()
    {
        VisibilityGenerator.AddRoomAndNeighbors(VisibleCoords, TargetRoom);
        Debug.Log($"Generated visibility data for room {TargetRoom.Type}. Total visible coords: {VisibleCoords.Count}");
    }

    public static VisibilityData? Get(Player player, bool allowDebug = true)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        return Get(player.Position, allowDebug);
    }

    public static VisibilityData? Get(Vector3 position, bool allowDebug = true)
    {
        Room room = Room.Get(position);

        if (room is null)
            return null;

        return Get(room, allowDebug);
    }

    public static VisibilityData Get(Room room, bool allowDebug = true)
    {
        if (room is null)
            throw new ArgumentNullException(nameof(room));

        Vector3Int coords = room.Identifier.MainCoords;

        if (!Cache.TryGetValue(coords, out VisibilityData? data))
        {
            data = new(room);
            Cache[room.Identifier.MainCoords] = data;
        }

        if (Config.Instance.DebugPrimitives && allowDebug)
            VisibilityDataDebugVisualizer.DrawDebugPrimitives(data);

        return data;
    }
}