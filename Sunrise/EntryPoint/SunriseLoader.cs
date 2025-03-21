using Sunrise.Features.CustomVisibility;
using Sunrise.Features.ServersideBacktrack;
using Sunrise.Features.ServersideTeslaDamage;
using Sunrise.Utility;

namespace Sunrise.EntryPoint;

public class SunriseLoader : PluginModule
{
    public override List<PluginModule> SubModules { get; } =
    [
        new CustomVisibilityModule(),
        new ServersideBacktrackModule(),
        new ServersideTeslaDamageModule(),
    ];
}