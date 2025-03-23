using System;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items.Usables;

namespace Sunrise.Features.PickupEspClutter;

public class PickupEspClutterModule : PluginModule
{
    protected override void OnEnabled()
    {
        Handlers.Server.RoundStarted += OnRoundStarted;
        Handlers.Server.RoundEnded += OnRoundEnded;
    }

    protected override void OnDisabled()
    {
        Handlers.Server.RoundStarted -= OnRoundStarted;
        Handlers.Server.RoundEnded -= OnRoundEnded;
    }

    static void OnRoundStarted()
    {
        PhantomItemSpawner.Start();
    }

    static void OnRoundEnded(RoundEndedEventArgs ev)
    {
        PhantomItemSpawner.Stop();
    }
}