using System;
using System.Linq;
using Exiled.API.Enums;
using MapGeneration;
using Sunrise.Utility;

namespace Sunrise.Features.AntiWallhack;

/// <summary>
///     Represents a collection of all coordinates that should be visible to a player in a specific room
/// </summary>
public class RoomVisibilityData
{
    static readonly Dictionary<Vector3Int, RoomVisibilityData> RoomVisibilityCache = new();

    readonly HashSet<Vector3Int> _visibleCoords = [];
    readonly Color _color = Random.ColorHSV(0, 1, 0.7f, 1, 0.7f, 1) * 50;
    float _lastPrimitiveSpawnTime = Time.time;

    RoomVisibilityData(Room room)
    {
        Include(room);

        Vector3Int roomCoords = RoomIdUtils.PositionToCoords(room.Position);
        Vector3Int[] directions = GetDirections(room, out bool known);

        foreach (Vector3Int direction in directions)
        {
            IncludeDirection(roomCoords, direction, known);
        }

        foreach (Room nearestRoom in room.NearestRooms)
        {
            Include(nearestRoom);
            Vector3Int nearestRoomCoords = RoomIdUtils.PositionToCoords(nearestRoom.Position);
            IncludeDirection(nearestRoomCoords, roomCoords - nearestRoomCoords, true);
        }

        Debug.Log($"Generated visibility data for room {room.Type}. Total visible coords: {_visibleCoords.Count}");
    }

    void Include(Room room)
    {
        _visibleCoords.UnionWith(room.Identifier.OccupiedCoords);

        if (DiagonalVisibilityRooms.Contains(room.Type))
        {
            Vector3Int coords = RoomIdUtils.PositionToCoords(room.Position);

            foreach (Vector3Int direction in GetDirections(room, out _))
            {
                Vector3Int neighbourCoords = coords + direction;
                _visibleCoords.Add(neighbourCoords);
            }
        }
    }

    Vector3Int[] GetDirections(Room room, out bool known)
    {
        if (KnownDirectionsRooms.TryGetValue(room.Type, out Vector3Int[] directions))
        {
            known = true;

            return directions.Select(d =>
            {
                Vector3 direction = room.Rotation * d;
                return new Vector3Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y), Mathf.RoundToInt(direction.z));
            }).ToArray();
        }
        else
        {
            known = false;
            return Directions;
        }
    }

    // Includes all rooms in a specified direction
    void IncludeDirection(Vector3Int coords, Vector3Int direction, bool known)
    {
        if (direction.sqrMagnitude != 1)
            throw new ArgumentException("Direction must be normalized");

        Vector3Int previousCoords = coords;
        coords += direction;

        while (RoomIdentifier.RoomsByCoordinates.TryGetValue(coords, out RoomIdentifier? roomIdentifier) && Room.Get(roomIdentifier) is Room room && (CheckConnection(previousCoords, coords) || known))
        {
            // When direction is known we always include the first room
            known = false;

            Include(room);
            previousCoords = coords;
            coords += direction;
        }
    }

    public bool CheckVisibility(Vector3Int coords) => _visibleCoords.Contains(coords);

    // Rooms get added into 'NearestRooms' only when there is a door between them, so we need a manual way of checking room connections
    static bool CheckConnection(Vector3Int coordsA, Vector3Int coordsB)
    {
        if (Vector3Int.Distance(coordsA, coordsB) > 1)
            return false;

        Vector3 posA = RoomIdUtils.CoordsToCenterPos(coordsA) + Vector3.up;
        Vector3 posB = RoomIdUtils.CoordsToCenterPos(coordsB) + Vector3.up;
        Vector3 direction = (posB - posA).normalized;

        foreach (float rightOffset in ConnectionCheckingOffsets)
        {
            // Translate the offset to local space of the direction
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(direction, up);
            Vector3 offset = right * rightOffset;

            if (!Physics.Linecast(posA, posA + offset, (int)Mask.DefaultColliders))
            {
                if (!Physics.Linecast(posA + offset, posB + offset - direction * 7.4f, (int)Mask.DefaultColliders))
                    return true;
            }

            if (!Physics.Linecast(posB, posB + offset, (int)Mask.DefaultColliders))
            {
                if (!Physics.Linecast(posB + offset, posA + offset + direction * 7.4f, (int)Mask.DefaultColliders))
                    return true;
            }
        }

        return false;
    }

    // Gets cached visibility data or creates a new one
    public static RoomVisibilityData? Get(Vector3Int coords)
    {
        if (!RoomVisibilityCache.TryGetValue(coords, out RoomVisibilityData? data))
        {
            Room room = Room.Get(RoomIdUtils.CoordsToCenterPos(coords));

            if (room != null)
            {
                try
                {
                    data = new RoomVisibilityData(room);

                    foreach (Vector3Int occupiedCoords in room.Identifier.OccupiedCoords)
                        RoomVisibilityCache[occupiedCoords] = data;
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to generate visibility data for room {room.Type}: {e}");
                }
            }
        }

        const float PrimitveDuration = 15;

        if (Config.Instance.DebugPrimitives && data is not null && data._lastPrimitiveSpawnTime + PrimitveDuration < Time.time)
        {
            data._lastPrimitiveSpawnTime = Time.time;

            Vector3 offset = Random.insideUnitSphere * 0.1f + Vector3.up * 0.3f;
            Debug.DrawPoint(RoomIdUtils.CoordsToCenterPos(coords) + offset, data._color, PrimitveDuration);

            foreach (Vector3Int visibleCoords in data._visibleCoords)
                Debug.DrawLine(RoomIdUtils.CoordsToCenterPos(coords) + offset, RoomIdUtils.CoordsToCenterPos(visibleCoords) + offset, data._color, PrimitveDuration);
        }

        return data;
    }

    #region Hardcoded stuff

    // Rooms that have known connection directions
    public static readonly Dictionary<RoomType, Vector3Int[]> KnownDirectionsRooms = new()
    {
        [RoomType.HczCornerDeep] = [Vector3Int.back, Vector3Int.right],
        [RoomType.HczNuke] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomType.HczCrossRoomWater] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right],
        [RoomType.HczArmory] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomType.HczIntersectionJunk] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],

        [RoomType.Hcz079] = [Vector3Int.left],
        [RoomType.HczIntersection] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomType.HczHid] = [Vector3Int.left, Vector3Int.right],
    };

    public static readonly HashSet<RoomType> DiagonalVisibilityRooms =
    [
        RoomType.HczCornerDeep,
        RoomType.HczNuke,
        RoomType.HczCrossRoomWater,
        RoomType.HczArmory,
        RoomType.HczIntersectionJunk,
    ];

    public static readonly Vector3Int[] Directions =
    [
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
    ];

    static readonly float[] ConnectionCheckingOffsets =
    [
        0,
        -1,
        1,
    ];

    #endregion
}