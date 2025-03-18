using Sunrise.Features.ServersideBacktrack;
using Sunrise.Utility;

namespace Sunrise.EntryPoint;

public class SunriseLoader : PluginModule
{
    public override List<PluginModule> SubModules { get; } =
    [
        new ServersideBacktrackModule(),
    ];
}