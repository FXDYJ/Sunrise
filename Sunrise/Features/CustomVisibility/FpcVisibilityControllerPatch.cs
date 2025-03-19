using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Exiled.API.Enums;
using HarmonyLib;
using JetBrains.Annotations;
using MapGeneration;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;

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
    public static readonly HashSet<RoomType> WhitelistedRooms =
    [
        // Common rooms
        RoomType.EzStraight,
        RoomType.HczStraight,
        RoomType.LczStraight,

        RoomType.EzTCross,
        RoomType.LczTCross,

        RoomType.EzCurve,
        RoomType.HczCurve,
        RoomType.LczCurve,

        // Unique to zones
        RoomType.EzShelter,

        RoomType.HczArmory,
        RoomType.HczTesla,

        RoomType.LczAirlock,
        RoomType.LczArmory,

        // Unique rooms
        RoomType.EzDownstairsPcs,
        RoomType.EzIntercom,
        RoomType.EzCollapsedTunnel,
        RoomType.EzSmallrooms,

        RoomType.Lcz914,
        RoomType.LczToilets,
        RoomType.LczCheckpointA,
        RoomType.LczCheckpointB,
        RoomType.LczClassDSpawn,
    ];

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = instructions.ToList();

        newInstructions.InsertRange(newInstructions.Count - 1,
        [
            new(OpCodes.Ldloc_2), // currentRole1
            new(OpCodes.Ldloc, 4), // position2
            new(OpCodes.Ldloc, 6), // V_5 (distanceSqr)
            new(OpCodes.Call, Method(typeof(VisibilityPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

    public static float CrossArmWidth = 9;
    public static float RoomGridSize = 15;

    /// <summary>
    ///     This method limits visibility diagonally when players are inside the facility.
    /// </summary>
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole role1, Vector3 position2, float distanceSqr)
    {
        // players are either out of range or confirmed in range
        if ((flags & InvisibilityFlags.OutOfRange) != 0 || distanceSqr < CrossArmWidth * CrossArmWidth)
            return flags;
        
        Vector3Int cords1 = RoomIdUtils.PositionToCoords(role1.FpcModule.Position);
        Vector3Int cords2 = RoomIdUtils.PositionToCoords(position2);

        if (Math.Abs(position1.z - position2.z) > CrossArmWidth)
        {
            if (Math.Abs(position1.y - position2.y) > CrossArmWidth)
            {
                if (Room.Get(position1) is Room room1 && WhitelistedRooms.Contains(room1.Type) && Room.Get(position2) is Room room2 && WhitelistedRooms.Contains(room2.Type))
                    return flags | InvisibilityFlags.OutOfRange;
            }
        }

        return flags;
    }
}