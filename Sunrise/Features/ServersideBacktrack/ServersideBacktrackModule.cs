using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using Sunrise.Utility;

namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     Serverside backtrack works by recording a precise history of player positions and rotations.
///     When a player shoots, the server finds the best values from the history instead of blindly trusting the client over their position and rotation values.
///     This prevents any cheats that include shooting in a different direction than the actual one.
/// </summary>
public class ServersideBacktrackModule : PluginModule
{
    public static readonly Dictionary<Player, BacktrackHistory> BacktrackHistories = new();

    protected override void OnEnabled()
    {
        StaticUnityMethods.OnUpdate += OnUpdate;
        Handlers.Player.ChangingItem += OnChangingItem;
    }

    protected override void OnDisabled()
    {
        StaticUnityMethods.OnUpdate -= OnUpdate;
        Handlers.Player.ChangingItem -= OnChangingItem;
    }

    protected override void OnReset()
    {
        BacktrackHistories.Clear();
    }

    static void OnUpdate()
    {
        foreach (Player player in Player.Dictionary.Values)
        {
            // No point in backtracking players that are not going to shoot
            if (player.CurrentItem is Firearm)
            {
                BacktrackHistory history = BacktrackHistories.GetOrAdd(player, () => new(player));
                history.RecordEntry();
            }
        }
    }

    static void OnChangingItem(ChangingItemEventArgs ev)
    {
        // Prevent old entries from being used
        if (ev.Item is Firearm && BacktrackHistories.TryGetValue(ev.Player, out BacktrackHistory history))
            history.Entries.Clear();
    }
}