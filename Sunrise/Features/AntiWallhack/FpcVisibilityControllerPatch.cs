using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Visibility;
using Sunrise.Utility;

namespace Sunrise.Features.AntiWallhack;

/*
InvisibilityFlags GetActiveFlags(ReferenceHub observer)
.locals init
[0] valuetype PlayerRoles.Visibility.InvisibilityFlags activeFlags,
[1] class PlayerRoles.FirstPersonControl.IFpcRole currentRole1,
[2] class PlayerRoles.FirstPersonControl.IFpcRole currentRole2,
[3] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 position2,
[4] float32 num,
[5] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 V_5
*/
[HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.GetActiveFlags))] [UsedImplicitly]
public static class VisibilityPatch
{
    [UsedImplicitly]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = instructions.ToList();

        newInstructions.InsertRange(newInstructions.Count - 1,
        [
            // activeFlags are already on the stack ready to be returned
            new(OpCodes.Ldloc_1), // currentRole1 (observer)
            new(OpCodes.Ldloc_2), // currentRole2 (target)
            new(OpCodes.Call, Method(typeof(VisibilityPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

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
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole observerRole, IFpcRole targetRole)
    {
        // Players are out of range
        if (!Config.Instance.AntiWallhack || (flags & InvisibilityFlags.OutOfRange) != 0)
            return flags;

        Vector3 observerPosition = observerRole.FpcModule.Position;
        Vector3 targetPosition = targetRole.FpcModule.Position;

        if (IsExceptionalCase(observerRole, targetRole))
            return flags;

        if (MathExtensions.SqrDistance(observerPosition, targetPosition) < GetForcedVisibilitySqrDistance(targetRole))
            return flags;

        Vector3Int observerCoords = RoomIdUtils.PositionToCoords(observerPosition);
        Vector3Int targetCoords = RoomIdUtils.PositionToCoords(targetPosition);

        if (RoomVisibilityData.Get(observerCoords) is not RoomVisibilityData visibilityData || visibilityData.CheckVisibility(targetCoords))
            return flags;

        return flags | InvisibilityFlags.OutOfRange;
    }

    static bool IsExceptionalCase(IFpcRole observerRole, IFpcRole targetRole)
    {
        if (observerRole.FpcModule.Noclip.IsActive)
            return true;

        // Scp049's sense ability allows to see the target through walls
        if (observerRole is Scp049Role scp049Role && scp049Role.SubroutineModule.TryGetSubroutine<Scp049SenseAbility>(out Scp049SenseAbility? sense) && sense.HasTarget && sense.Target == targetRole.FpcModule.Hub)
            return true;

        // Scp939 can hear players through walls and has its own surprisingly robust VisibilityController
        if (observerRole is Scp939Role)
            return true;

        // Remarks:
        // Scp096 ignores OutOfRange flag when enraged

        return false;
    }
}