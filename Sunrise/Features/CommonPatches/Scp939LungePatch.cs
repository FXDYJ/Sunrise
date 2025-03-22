using HarmonyLib;
using JetBrains.Annotations;
using MEC;
using Mirror;
using PlayerRoles.PlayableScps.Scp939;

namespace Sunrise.Features.CommonPatches;

// void ServerProcessCmd(NetworkReader reader)
[HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))] [UsedImplicitly]
public static class Scp939LungePatch
{
    [UsedImplicitly]
    public static void Prefix(NetworkReader reader, Scp939LungeAbility __instance)
    {
        Timing.CallDelayed(2, () =>
        {
            if (__instance.State == Scp939LungeState.Triggered)
            {
                __instance.State = Scp939LungeState.None;
            }
        });
    }
}