using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Exiled.API.Enums;
using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using JetBrains.Annotations;
using MapGeneration;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;
using Sunrise.API.Visibility;
using Sunrise.Features.AntiWallhack.ForcedVisibility;
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
[HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.GetActiveFlags))] [UsedImplicitly]
internal static class FpcVisibilityControllerPatch
{
    static readonly AutoBenchmark Benchmark = new("Anti Wallhack (without raycasts)");

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
        try
        {
            if (!Config.Instance.AntiWallhack)
                return flags;

            // Players are out of range
            if ((flags & InvisibilityFlags.OutOfRange) != 0)
                return flags;

            Benchmark.Start();

            Player observer = Player.Get(observerRole.FpcModule.Hub);
            Player target = Player.Get(targetRole.FpcModule.Hub);

            if (IsExceptionalCase(observer, observerRole, target))
            {
                Benchmark.Stop();
                return flags;
            }

            float sqrDistance = positionDifference.sqrMagnitude;
            float forcedVisibility = ForcedVisibilityHelper.GetForcedVisibility(target);

            if (sqrDistance < forcedVisibility * forcedVisibility)
            {
                Benchmark.Stop();
                return flags;
            }

            if (VisibilityData.Get(observerRole.FpcModule.Position) is VisibilityData observerVisibility && !observerVisibility.IsVisible(target))
            {
                Benchmark.Stop();
                return flags | InvisibilityFlags.OutOfRange;
            }

            Benchmark.Stop();

            if (Config.Instance.RaycastAntiWallhack && !RaycastVisibilityChecker.IsVisible(observer, target))
                return flags | InvisibilityFlags.OutOfRange;

            return flags;
        }
        catch (Exception e)
        {
            Log.Error($"Error in {nameof(FpcVisibilityControllerPatch)}.{nameof(AddCustomVisibility)}: {e}");
            return flags;
        }
    }

    static bool IsExceptionalCase(Player observer, IFpcRole observerRole, Player target)
    {
        if (observerRole.FpcModule.Noclip.IsActive)
            return true;

        switch (target.Inventory.CurInstance) // Light emitting items make players visible
        {
            // Night vision scopes count as emitting light (nw classic), so firearms need special handling
            case Firearm firearm:
            {
                foreach (ILightEmittingItem lightEmitter in firearm._modifiersCombiner._lightEmitters)
                {
                    if (lightEmitter is FlashlightAttachment { IsEmittingLight: true })
                        return true;
                }

                return false;
            }
            case ILightEmittingItem { IsEmittingLight: true }:
            {
                return true;
            }
        }

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