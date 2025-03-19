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
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = instructions.ToList();

        newInstructions.InsertRange(newInstructions.Count - 1,
        [
            new(OpCodes.Ldloc_1), // currentRole1
            new(OpCodes.Ldloc, 3), // position2
            new(OpCodes.Call, Method(typeof(VisibilityPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

    public static bool Enabled = false;
    public static float SmallerRoomGridSize = 9;
    static int counter = 0;

    /// <summary>
    ///     This method limits visibility diagonally when players are inside the facility.
    /// </summary>
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole role1, Vector3 position2)
    {
        // players are out of range
        if (!Enabled || (flags & InvisibilityFlags.OutOfRange) != 0)
            return flags;

        Vector3 position1 = role1.FpcModule.Position;
        Vector3Int cords1 = RoomIdUtils.PositionToCoords(position1);
        Vector3Int cords2 = RoomIdUtils.PositionToCoords(position2);

        // If rooms are diagonal to each other
        if (cords1.x != cords2.x && cords1.z != cords2.z)
        {
            if (counter++ % 30 == 0)
                Log.Warn($"{role1.FpcModule?.Hub?.nicknameSync?.MyNick} cant see - Position1: {position1}, Position2: {position2}, Cords1: {cords1}, Cords2: {cords2}");

            return flags | InvisibilityFlags.OutOfRange;
        }

        if (counter++ % 30 == 0)
            Log.Warn($"{role1.FpcModule?.Hub?.nicknameSync?.MyNick} - Position1: {position1}, Position2: {position2}, Cords1: {cords1}, Cords2: {cords2}");

        return flags;
    }
}