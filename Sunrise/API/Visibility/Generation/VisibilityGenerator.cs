using System;
using MapGeneration;

namespace Sunrise.API.Visibility.Generation;

internal static class VisibilityGenerator
{
    internal static void AddRoomAndNeighbors(HashSet<Vector3Int> visibleCoords, Room room)
    {
        if (room is null)
            throw new ArgumentNullException(nameof(room));

        if (visibleCoords is null)
            throw new ArgumentNullException(nameof(visibleCoords));

        IncludeRoom(visibleCoords, room);
        ProcessRoomDirections(visibleCoords, room);
        ProcessNearestRooms(room, visibleCoords);
    }

    static void IncludeRoom(HashSet<Vector3Int> visibleCoords, Room room)
    {
        visibleCoords.Add(room.Identifier.MainCoords);

        if (RoomVisibilityConfig.DiagonalVisibilityRooms.Contains(room.Type))
        {
            AddDiagonalNeighbors(visibleCoords, room);
        }
    }

    static void AddDiagonalNeighbors(HashSet<Vector3Int> visibleCoords, Room room)
    {
        Vector3Int coords = RoomUtils.PositionToCoords(room.Position);

        foreach (Vector3Int direction in RoomDirectionHelper.GetSearchDirections(room, out _))
        {
            visibleCoords.Add(coords + direction);
        }
    }

    static void ProcessRoomDirections(HashSet<Vector3Int> visibleCoords, Room room)
    {
        Vector3Int roomCoords = RoomUtils.PositionToCoords(room.Position);
        Vector3Int[] directions = RoomDirectionHelper.GetSearchDirections(room, out bool known);

        foreach (Vector3Int direction in directions)
        {
            RoomDirectionHelper.IncludeDirection(roomCoords, direction, known, visibleCoords);
        }
    }

    static void ProcessNearestRooms(Room room, HashSet<Vector3Int> visibleCoords)
    {
        foreach (Room nearestRoom in room.NearestRooms)
        {
            IncludeRoom(visibleCoords, nearestRoom);
            Vector3Int nearestRoomCoords = RoomUtils.PositionToCoords(nearestRoom.Position);
            Vector3Int direction = RoomUtils.PositionToCoords(room.Position) - nearestRoomCoords;
            RoomDirectionHelper.IncludeDirection(nearestRoomCoords, direction, true, visibleCoords);
        }
    }
}