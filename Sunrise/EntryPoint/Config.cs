using System.ComponentModel;
using Exiled.API.Interfaces;
using JetBrains.Annotations;

namespace Sunrise.EntryPoint;

[UsedImplicitly]
public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;

    [Description("Enables some visual debugging features")]
    public bool DebugPrimitives { get; set; } = false;

    [Description(
        """
        The maximum latency for which the server has to account for.
        Higher values give more authority to clients, lower values may decrease gameplay quality for players with higher latency.
        """
    )]
    public float AccountedLatencySeconds { get; set; } = 0.3f;

    [Category("Toggle features separately")]
    [Description("Limits the information sent to players. Performance impact not yet measured.")]
    public bool AntiWallhack { get; set; } = true;

    [Description("Enables the serverside door damage feature. Preformance impact negligible.")]
    public bool PickupValidation { get; set; } = true;

    [Description("Enables the serverside backtracking feature. Improves performance.")]
    public bool ServersideBacktrack { get; set; } = true;

    [Description("Enables the serverside tesla damage feature. Preformance impact negligible.")]
    public bool ServersideTeslaDamage { get; set; } = true;

    #region Singleton

    public Config() => Instance = this;
    public static Config Instance { get; private set; } = null!;

    #endregion
}