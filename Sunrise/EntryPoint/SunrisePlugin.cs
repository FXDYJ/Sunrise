using System;
using HarmonyLib;
using JetBrains.Annotations;

namespace Sunrise.EntryPoint;

[UsedImplicitly]
public class SunrisePlugin : Plugin<Config>
{
    public override string Name { get; } = "Sunrise";
    public override string Author { get; } = "BanalnyBanan";

    public SunriseLoader Loader { get; } = new();
    public Harmony Harmony { get; } = new("Sunrise");
    public static SunrisePlugin Instance { get; private set; } = null!;

    public override void OnEnabled()
    {
        Instance = this;
        Loader.Enable();
        Harmony.PatchAll();
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Loader.Disable();
        Harmony.UnpatchAll();
        base.OnDisabled();
    }
}