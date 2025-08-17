using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MapGeneration;

namespace Sunrise.Features.PickupEspClutter;

internal static class PhantomPickupSynchronizer
{
    static readonly HashSet<RoomName> ExcludedRooms =
    [
        RoomName.Outside,

        RoomName.Hcz106,
        RoomName.HczCrossing,
        RoomName.HczStraight,
        RoomName.HczNuke,
        RoomName.HczHid,

        RoomName.Lcz173,
    ];

    [field: AllowNull, MaybeNull]
    static List<RoomIdentifier> Rooms => field ??= RoomIdentifier.AllRoomIdentifiers.Where(r => !ExcludedRooms.Contains(r.Name)).ToList();

    static int index;

    internal static void GetNextPosition(out Vector3 position)
    {
        RoomIdentifier room = Rooms[index];

        const float RandomOffset = 1.5f;
        position = room.transform.position + (Random.insideUnitSphere * RandomOffset) with { y = Random.Range(10, 15) };

        index = (index + 1) % Rooms.Count;
    }
}