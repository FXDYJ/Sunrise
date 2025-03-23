using System;
using Exiled.Events.EventArgs.Map;

namespace Sunrise.Features.PickupEspClutter;

public class PickupEspClutterModule : PluginModule
{
    protected override void OnEnabled()
    {
        Handlers.Map.PickupAdded += OnPickupAdded;
    }

    protected override void OnDisabled()
    {
        Handlers.Map.PickupAdded -= OnPickupAdded;
    }

    void OnPickupAdded(PickupAddedEventArgs ev)
    {
        ev.Pickup.Info = ev.Pickup.Info with { ItemId = ItemType.Coin };
    }
}

public class EspClutterGridManager { }