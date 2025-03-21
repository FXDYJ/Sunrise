using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Sunrise.EntryPoint;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;

    [Description("Enables some visualizations for debugging")]
    public bool DebugPrimitives { get; set; } = false;

    [Description("Toggle features separately")]
    public bool CustomVisibility { get; set; } = true;
    public bool ServersideBacktrack { get; set; } = true;
    public bool ServersideTeslaDamage { get; set; } = true;
}