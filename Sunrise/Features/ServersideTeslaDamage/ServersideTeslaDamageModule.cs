using System;
using PluginAPI.Events;
using PluginAPI.Events.Arguments;
using PlayerStatsSystem;

namespace Sunrise.Features.ServersideTeslaDamage;

/// <summary>
///     Makes tesla gates deal damage on server side after a short delay to account for latency.
///     In base game, clients are expected to report themselves getting damaged, which cheaters can exploit.
/// </summary>
internal class ServersideTeslaDamageModule : PluginModule
{
    protected override void OnEnabled()
    {
        TeslaGate.OnBursted += OnTeslaGateBursted;
        EventManager.RegisterEvents<PlayerEvents>(this);
    }

    protected override void OnDisabled()
    {
        TeslaGate.OnBursted -= OnTeslaGateBursted;
        EventManager.UnregisterEvents<PlayerEvents>(this);
    }

    protected override void OnReset()
    {
        ServersideTeslaHitreg.Dictionary.Clear();
        ServersideTeslaHitreg.ShockedPlayers.Clear();
    }

    static void OnTeslaGateBursted(TeslaGate tesla)
    {
        if (!Config.Instance.ServersideTeslaDamage)
            return;

        if (tesla == null)
            return;

        try
        {
            ServersideTeslaHitreg.Get(tesla).Burst();
        }
        catch (Exception e)
        {
            Log.Error($"Error in {nameof(ServersideTeslaDamageModule)}.{nameof(OnTeslaGateBursted)}: {e}");
        }
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerDamage)]
    void OnPlayerHurt(PlayerDamageEventArgs ev)
    {
        // Check if damage is from Tesla
        if (ev.DamageHandler is ElectrocutionDamageHandler)
            ServersideTeslaHitreg.ShockedPlayers[ev.Target] = Time.time;
    }
}