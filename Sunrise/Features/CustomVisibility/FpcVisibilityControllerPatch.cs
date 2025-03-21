using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Exiled.API.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;
using Sunrise.EntryPoint;
using Sunrise.Utility;
using UnityEngine.ProBuilder;

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
            new(OpCodes.Ldloc_2), // currentRole2 (target)
            new(OpCodes.Call, Method(typeof(VisibilityPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

    public static float ForcedVisibilitySqrDistance = 10f * 10f;
    public static float VisibilityLimit = 50f;

    // Some roles' footstep sounds can be heard from a distance higher than human 12m forced visibility distance
    static float GetForcedVisibilitySqrDistance(IFpcRole role) => role.FpcModule.Role.RoleTypeId switch
    {
        RoleTypeId.Scp939 or RoleTypeId.Scp173 => 5000,
        RoleTypeId.Scp106 => 29 * 29,
        _ => 12 * 12,
    };

    /// <summary>
    ///     This method limits visibility diagonally when players are inside the facility.
    /// </summary>
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole role1, IFpcRole role2)
    {
        // players are out of range
        if (!SunrisePlugin.Instance.Config.CustomVisibility || (flags & InvisibilityFlags.OutOfRange) != 0)
            return flags;

        Vector3 position1 = role1.FpcModule.Position;
        Vector3 position2 = role2.FpcModule.Position;

        if (MathExtensions.SqrDistance(position1, position2) < GetForcedVisibilitySqrDistance(role2) || FpcNoclip.IsPermitted(role1.FpcModule.Hub))
            return flags;

        if (Mathf.Abs(position1.y - position2.y) > VisibilityLimit)
            return flags | InvisibilityFlags.OutOfRange;

        Vector3Int coords1 = RoomIdUtils.PositionToCoords(position1); // observer
        Vector3Int coords2 = RoomIdUtils.PositionToCoords(position2); // target

        if (RoomVisibilityData.Get(coords1) is not RoomVisibilityData visibilityData || visibilityData.CheckVisibility(coords2))
            return flags;

        return flags | InvisibilityFlags.OutOfRange;
    }
}