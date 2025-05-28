using Exiled.Events.EventArgs.Player;

namespace Sunrise.Features.AntiWallhack;

internal class AntiWallhackModule : PluginModule
{
    // After player lands their visibility should stay on for some time
    internal static readonly Dictionary<Player, float> LandingTimes = new();

    protected override void OnEnabled()
    {
        Handlers.Player.Landing += OnPlayerLanding;
    }

    protected override void OnDisabled()
    {
        Handlers.Player.Landing -= OnPlayerLanding;
    }

    protected override void OnReset()
    {
        LandingTimes.Clear();
    }

    static void OnPlayerLanding(LandingEventArgs ev)
    {
        if (ev.Player is not null)
        {
            LandingTimes[ev.Player] = Time.time;
        }
    }
}