using PluginAPI.Events;
using PluginAPI.Events.Arguments;

namespace Sunrise.Features.AntiWallhack;

internal class AntiWallhackModule : PluginModule
{
    // After player lands their visibility should stay on for some time
    internal static readonly Dictionary<Player, float> LandingTimes = new();

    protected override void OnEnabled()
    {
        EventManager.RegisterEvents<PlayerEvents>(this);
    }

    protected override void OnDisabled()
    {
        EventManager.UnregisterEvents<PlayerEvents>(this);
    }

    protected override void OnReset()
    {
        LandingTimes.Clear();
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerLanding)]
    void OnPlayerLanding(PlayerLandingEventArgs ev)
    {
        if (ev.Player is not null)
        {
            LandingTimes[ev.Player] = Time.time;
        }
    }
}