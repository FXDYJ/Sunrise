using System.Linq;
using Exiled.API.Enums;
using MapGeneration;

namespace Sunrise.Features.CustomVisibility;

public class RoomVisibilityData
{
    // Some rooms can be seen through diagonally
    static readonly HashSet<RoomType> ExtraVisibilityRooms =
    [
        RoomType.HczNuke,
        RoomType.HczCornerDeep,
        RoomType.HczCrossRoomWater,
        RoomType.HczArmory,
    ];

    public readonly HashSet<Vector3Int> VisibleRooms;
    public Vector3Int RoomCords;

    public RoomVisibilityData(Room room)
    {
        RoomCords = RoomIdUtils.PositionToCoords(room.Position);
        VisibleRooms = [RoomCords];

        foreach (Room nearestRoom in room.NearestRooms)
        {
            Vector3Int direction = RoomIdUtils.PositionToCoords(nearestRoom.Position) - RoomCords;
            Vector3Int cords = RoomCords + direction;

            if (ExtraVisibilityRooms.Contains(nearestRoom.Type))
            {
                VisibleRooms.UnionWith(nearestRoom.NearestRooms.Select(r => RoomIdUtils.PositionToCoords(r.Position)));
            }

            while (RoomIdentifier.RoomsByCoordinates.TryGetValue(cords, out RoomIdentifier nextRoom))
            {
                VisibleRooms.Add(cords);
                cords += direction;
            }
        }
    }
}