using Exiled.API.Enums;
using MapGeneration;
using Sunrise.EntryPoint;
using Random = UnityEngine.Random;

namespace Sunrise.Features.CustomVisibility;

public class RoomVisibilityData
{
    // Some rooms can be seen through diagonally
    public static readonly HashSet<RoomType> ExtraVisibilityRooms =
    [
        RoomType.HczNuke,
        RoomType.HczCornerDeep,
        RoomType.HczCrossRoomWater,
        RoomType.HczArmory,
    ];

    public static readonly List<Vector3Int> Directions =
    [
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
    ];

    public static readonly int DefaultCollidersLayerMask = 1 << 0;
    static readonly Dictionary<Vector3Int, RoomVisibilityData> RoomVisibilityCache = new();

    readonly HashSet<Vector3Int> _visibleCoords = [];

    RoomVisibilityData(Room room)
    {
        Log.Info($"Generating visibility data for room {room.Type}");

        Include(room);
        Vector3Int roomCoords = RoomIdUtils.PositionToCoords(room.Position);

        foreach (Vector3Int direction in Directions)
        {
            IncludeDirection(roomCoords, direction);
        }

        foreach (Room nearestRoom in room.NearestRooms)
        {
            Include(nearestRoom);
            Vector3Int nearestRoomCoords = RoomIdUtils.PositionToCoords(nearestRoom.Position);
            IncludeDirection(nearestRoomCoords, roomCoords - nearestRoomCoords);
        }

        if (SunrisePlugin.Instance.Config.DebugPrimitives)
        {
            Color color = Random.ColorHSV(0, 1, 0.7f, 1, 0.7f, 1) * 50;
            Vector3 offset = Random.insideUnitSphere * 0.1f + Vector3.up * 0.3f;

            foreach (Vector3Int coords in _visibleCoords)
            {
                Debug.DrawLine(RoomIdUtils.CoordsToCenterPos(roomCoords) + offset, RoomIdUtils.CoordsToCenterPos(coords) + offset, color, float.MaxValue);
            }

            Log.Warn($"Generated visibility data for room {room.Type}. Total visible coords: {_visibleCoords.Count}");
        }
    }

    void Include(Room room)
    {
        _visibleCoords.UnionWith(room.Identifier.OccupiedCoords);

        if (ExtraVisibilityRooms.Contains(room.Type))
        {
            Vector3Int coords = RoomIdUtils.PositionToCoords(room.Position);

            foreach (Vector3Int direction in Directions)
            {
                _visibleCoords.Add(coords + direction);
            }
        }
    }

    void IncludeDirection(Vector3Int coords, Vector3Int direction)
    {
        Vector3Int previousCoords = coords;
        coords += direction;

        Log.Warn($"  Checking direction {direction}");

        while (RoomIdentifier.RoomsByCoordinates.TryGetValue(coords, out RoomIdentifier? roomIdentifier) && Room.Get(roomIdentifier) is Room room && CheckConnection(previousCoords, coords))
        {
            Log.Warn($"    Adding {coords}");
            Include(room);
            previousCoords = coords;
            coords += direction;
        }
    }

    public bool CheckVisibility(Vector3Int coords) => _visibleCoords.Contains(coords);

    static bool CheckConnection(Vector3Int coordsA, Vector3Int coordsB)
    {
        if (Vector3Int.Distance(coordsA, coordsB) > 1)
            return false;

        Vector3 posA = RoomIdUtils.CoordsToCenterPos(coordsA) + Vector3.up;
        Vector3 posB = RoomIdUtils.CoordsToCenterPos(coordsB) + Vector3.up;
        Vector3 direction = (posB - posA).normalized;

        if (!Physics.Linecast(posA, posB - direction * 7.4f, DefaultCollidersLayerMask))
            return true;

        if (!Physics.Linecast(posB, posA + direction * 7.4f, DefaultCollidersLayerMask))
            return true;

        return false;
    }

    public static RoomVisibilityData? Get(Vector3Int coords)
    {
        if (!RoomVisibilityCache.TryGetValue(coords, out RoomVisibilityData? data))
        {
            Room room = Room.Get(RoomIdUtils.CoordsToCenterPos(coords));

            if (room != null)
            {
                data = new RoomVisibilityData(room);

                foreach (Vector3Int occupiedCoords in room.Identifier.OccupiedCoords)
                {
                    RoomVisibilityCache[occupiedCoords] = data;
                }
            }
        }

        return data;
    }
}