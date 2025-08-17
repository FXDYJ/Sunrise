using MapGeneration;

namespace Sunrise.API.Visibility.Generation;

internal static class RoomVisibilityConfig
{
    public static readonly Dictionary<RoomName, Vector3Int[]> KnownDirectionsRooms = new()
    {
        [RoomName.HczCurve] = [Vector3Int.back, Vector3Int.right],
        [RoomName.HczNuke] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomName.HczCrossing] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right],
        [RoomName.HczArmory] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],

        [RoomName.Hcz079] = [Vector3Int.left],
        [RoomName.HczTCross] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomName.HczHid] = [Vector3Int.left, Vector3Int.right],
        [RoomName.Hcz939] = [Vector3Int.right, Vector3Int.back],

        [RoomName.EzStraight] = [Vector3Int.forward, Vector3Int.back],
        [RoomName.EzIntercom] = [Vector3Int.left, Vector3Int.back],
    };

    public static readonly HashSet<RoomName> DiagonalVisibilityRooms =
    [
        RoomName.HczCurve,
        RoomName.HczNuke,
        RoomName.HczCrossing,
        RoomName.HczArmory,
        RoomName.HczTCross,
    ];

    public static readonly Vector3Int[] DefaultDirections =
    [
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
    ];
}