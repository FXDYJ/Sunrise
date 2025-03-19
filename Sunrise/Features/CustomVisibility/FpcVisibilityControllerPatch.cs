using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Exiled.API.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using MapGeneration;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;
using Sunrise.Utility;

namespace Sunrise.Features.CustomVisibility;

// InvisibilityFlags GetActiveFlags(ReferenceHub observer)
/*
.locals init
[0] valuetype PlayerRoles.Visibility.InvisibilityFlags activeFlags,
[1] class PlayerRoles.FirstPersonControl.IFpcRole currentRole1,
[2] class PlayerRoles.FirstPersonControl.IFpcRole currentRole2,
[3] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 position2,
[4] float32 num,
[5] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 V_5
*/
[HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.GetActiveFlags))] [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class VisibilityPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = instructions.ToList();

        newInstructions.InsertRange(newInstructions.Count - 1,
        [
            new(OpCodes.Ldloc_1), // currentRole1 (observer)
            new(OpCodes.Ldloc, 3), // position2 (owner)
            new(OpCodes.Call, Method(typeof(VisibilityPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

    public static bool Enabled = false;
    public static float ForcedVisibilitySqrDistance = 12f * 12f;
    static int counter;

    /// <summary>
    ///     This method limits visibility diagonally when players are inside the facility.
    /// </summary>
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole role1, Vector3 position2)
    {
        // players are out of range
        if (!Enabled || flags.HasFlagFast(InvisibilityFlags.OutOfRange))
            return flags;

        Vector3 position1 = role1.FpcModule.Position;

        if (MathExtensions.SqrDistance(position1, position2) < ForcedVisibilitySqrDistance)
            return flags;

        Vector3Int coords1 = RoomIdUtils.PositionToCoords(position1); // observer
        Vector3Int coords2 = RoomIdUtils.PositionToCoords(position2); // other player

        if (RoomVisibilityData.Get(coords1) is not RoomVisibilityData visibilityData)
        {
            if (counter++ % 30 == 0)
                Log.Warn($"Failed to get visibility data for room {coords1}. Cords2: {coords2}");
            return flags;
        }

        if (!visibilityData.CheckVisibility(coords2))
        {
            if (counter++ % 30 == 0)
                Log.Warn($"Player at {coords1} can't see player at {coords2}");
            return flags | InvisibilityFlags.OutOfRange;
        }

        if (counter++ % 30 == 0)
            Log.Warn($"Player at {coords1} can see player at {coords2}");

        return flags;
    }

    public static HashSet<Vector3Int> GetVisibleCords(Room room)
    {
        Vector3Int roomCords = RoomIdUtils.PositionToCoords(room.Position);
        HashSet<Vector3Int> seeing = [roomCords];

        foreach (Room nearestRoom in room.NearestRooms)
        {
            Vector3Int direction = RoomIdUtils.PositionToCoords(nearestRoom.Position) - roomCords;
            Vector3Int cords = roomCords += direction;

            while (RoomIdentifier.RoomsByCoordinates.ContainsKey(cords))
            {
                seeing.Add(cords);
                cords += direction;
            }
        }

        return seeing;
    }
}