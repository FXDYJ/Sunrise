using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Exiled.API.Features.Items;
using HarmonyLib;
using JetBrains.Annotations;
using MapGeneration;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;
using Sunrise.API.Visibility;
using IVoiceRole = PlayerRoles.Voice.IVoiceRole;
using Scp049Role = Exiled.API.Features.Roles.Scp049Role;
using Scp939Role = Exiled.API.Features.Roles.Scp939Role;

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

/// <summary>
///     Wallhack nerf.
///     Works by only sending data about players in rooms that can be seen from the room the observer is currently in.
///     Reduces wallhack effective distance to around 12m (from 36m in base game)
/// </summary>
[HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.GetActiveFlags)), UsedImplicitly]
internal static class FpcVisibilityControllerPatch
{
    static int counter = 0;

    [UsedImplicitly]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = instructions.ToList();

        newInstructions.InsertRange(newInstructions.Count - 1,
        [
            // activeFlags are already on the stack ready to be returned

            new(OpCodes.Ldloc_1), // currentRole1 (observer)
            new(OpCodes.Ldloc_2), // currentRole2 (target)

            new(OpCodes.Ldloc_S, 5), // V_5 (position difference)

            new(OpCodes.Call, Method(typeof(FpcVisibilityControllerPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

    /// <summary>
    ///     This method limits visibility diagonally when players are inside the facility.
    /// </summary>
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole observerRole, IFpcRole targetRole, Vector3 positionDifference)
    {
        counter = (counter + 1) % 31;

        if (!Config.Instance.AntiWallhack)
            return flags;

        // Players are out of range
        if ((flags & InvisibilityFlags.OutOfRange) != 0)
            return flags;

        Player observer = Player.Get(observerRole.FpcModule.Hub);
        Player target = Player.Get(targetRole.FpcModule.Hub);

        if (IsExceptionalCase(observer, observerRole, target))
            return flags;

        float sqrDistance = positionDifference.sqrMagnitude;
        float forcedVisibility = ForcedVisibilityHelper.GetForcedVisibility(target);

        if (sqrDistance < forcedVisibility * forcedVisibility)
        {
            if (counter == 0)
            {
                Debug.Log($"+ Observer {observerRole.FpcModule.Hub.nicknameSync.MyNick} has forced visibility on {targetRole.FpcModule.Hub.nicknameSync.MyNick}. " +
                    $"Forced visibility distance: {forcedVisibility}m, distance: {Mathf.Sqrt(sqrDistance)}m");
            }

            return flags;
        }
        else
        {
            if (counter == 0)
            {
                Debug.Log($"- Observer {observerRole.FpcModule.Hub.nicknameSync.MyNick} doesn't have forced visibility on {targetRole.FpcModule.Hub.nicknameSync.MyNick}. " +
                    $"Forced visibility distance: {forcedVisibility}m, distance: {Mathf.Sqrt(sqrDistance)}m");
            }
        }

        Vector3Int observerCoords = RoomIdUtils.PositionToCoords(observerRole.FpcModule.Position);
        Vector3Int targetCoords = RoomIdUtils.PositionToCoords(targetRole.FpcModule.Position);

        if (VisibilityData.Get(observerCoords) is VisibilityData observerVisibility && !observerVisibility.IsVisible(targetCoords))
        {
            if (counter == 0)
                Debug.Log($"Observer {observerRole.FpcModule.Hub.nicknameSync.MyNick} can't see {targetRole.FpcModule.Hub.nicknameSync.MyNick} at {targetCoords} according to visibility data. sqrDistance: {sqrDistance}");

            return flags | InvisibilityFlags.OutOfRange;
        }

        if (Config.Instance.RaycastVisibilityValidation && !RaycastVisibilityChecker.IsVisible(Player.Get(observerRole.FpcModule.Hub), Player.Get(targetRole.FpcModule.Hub)))
        {
            if (counter == 0)
                Debug.Log($"Observer {observerRole.FpcModule.Hub.nicknameSync.MyNick} can't see {targetRole.FpcModule.Hub.nicknameSync.MyNick} at {targetCoords} according to raycasts. sqrDistance: {sqrDistance}");

            return flags | InvisibilityFlags.OutOfRange;
        }

        return flags;
    }

    static bool IsExceptionalCase(Player observer, IFpcRole observerRole, Player target)
    {
        if (observerRole.FpcModule.Noclip.IsActive)
            return true;

        if (target.CurrentItem is Firearm { FlashlightEnabled: true } or Flashlight { IsEmittingLight: true })
            return true;

        switch (observer.Role)
        {
            // Scp049's sense ability allows to see the target through walls
            case Scp049Role { SenseAbility: { HasTarget: true, Target: ReferenceHub senseTarget } } when senseTarget == target.ReferenceHub:

            // Scp939 can hear players through walls and has its own visibility system
            case Scp939Role:
                return true;

            // Scp096 ignores OutOfRange flag when enraged and has its own visibility system
        }

        return false;
    }
}