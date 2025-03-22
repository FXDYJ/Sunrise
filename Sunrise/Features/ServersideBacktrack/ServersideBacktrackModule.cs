using MapGeneration;
using Sunrise.EntryPoint;
using Sunrise.Features.CustomVisibility;
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
        Handlers.Server.ReloadedConfigs += OnReset;
    }

    protected override void OnDisabled()
    {
        StaticUnityMethods.OnUpdate -= OnUpdate;
        Handlers.Server.ReloadedConfigs -= OnReset;
    }

    protected override void OnReset()
    {
        BacktrackHistories.Clear();
    }

    static void OnUpdate()
    {
        if (!Config.Instance.ServersideBacktrack)
            return;

        foreach (Player player in Player.Dictionary.Values)
        {
            BacktrackHistory history = BacktrackHistories.GetOrAdd(player, () => new(player));
            history.RecordEntry();
        }
    }
}