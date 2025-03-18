using Exiled.API.Interfaces;

namespace Sunrise.EntryPoint;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;
    public bool DebugPrimitives { get; set; } = false;
}