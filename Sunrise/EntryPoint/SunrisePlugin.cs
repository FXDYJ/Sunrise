using System;
using HarmonyLib;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Interfaces;
using PluginAPI.Enums;

namespace Sunrise.EntryPoint;

public class SunrisePlugin : IPlugin<Config>
{
    [PluginConfig]
    public Config Config { get; set; } = new();

    public string Name { get; } = "Sunrise";
    public string Author { get; } = "BanalnyBanan";
    public string Version { get; } = "1.4.4";
    public string Description { get; } = "Anti-cheat plugin for SCP:SL servers";
    public PluginPriority Priority { get; } = PluginPriority.Higher;

    public SunriseLoader Loader { get; } = new();
    public Harmony Harmony { get; } = new("Sunrise");

    [PluginEntryPoint("Sunrise", "1.4.4", "Anti-cheat plugin for SCP:SL servers", "BanalnyBanan")]
    public void OnEnabled()
    {
        Loader.Enable();
        Harmony.PatchAll();
        Log.Info("Sunrise plugin has been enabled!");
    }

    [PluginUnload]
    public void OnDisabled()
    {
        Loader.Disable();
        Harmony.UnpatchAll();
        Log.Info("Sunrise plugin has been disabled!");
    }
}