using PluginAPI.Events;

namespace Sunrise.API.Backtracking;

/// <summary>
///     Serverside backtrack works by recording a precise history of player positions and rotations.
///     When a player shoots, the server finds the best values from the history instead of blindly trusting the client over their position and rotation values.
///     This prevents any cheats that include shooting in a different direction than the actual one.
/// </summary>
internal class BacktrackingModule : PluginModule
{
    protected override void OnEnabled()
    {
        EventManager.RegisterEvents<ServerEvents>(this);
    }

    protected override void OnDisabled()
    {
        EventManager.UnregisterEvents<ServerEvents>(this);
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.ReloadedConfigs)]
    void OnConfigReloaded()
    {
        OnReset();
    }

    protected override void OnReset()
    {
        BacktrackHistory.Dictionary.Clear();
    }
}