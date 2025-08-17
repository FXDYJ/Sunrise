using PluginAPI.Events;
using PluginAPI.Events.Arguments;

namespace Sunrise.Features.PickupValidation;

internal class PickupValidationModule : PluginModule
{
    protected override void OnEnabled()
    {
        EventManager.RegisterEvents<PlayerEvents>(this);
        EventManager.RegisterEvents<Scp914Events>(this);
    }

    protected override void OnDisabled()
    {
        EventManager.UnregisterEvents<PlayerEvents>(this);
        EventManager.UnregisterEvents<Scp914Events>(this);
    }

    protected override void OnReset()
    {
        PickupValidator.TemporaryPlayerBypass.Clear();
        PickupValidator.LockerLastInteraction.Clear();
        PickupValidator.DoorLastInteraction.Clear();
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.Scp914UpgradeInventory)]
    void OnScp914UpgradingInventoryItem(Scp914UpgradeInventoryEventArgs ev)
    {
        PickupValidator.TemporaryPlayerBypass[ev.Player] = Time.time + 0.01f;
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerInteractLocker)]
    void OnInteractingLocker(PlayerInteractLockerEventArgs ev)
    {
        if (ev.CanInteract)
            PickupValidator.LockerLastInteraction[ev.Locker] = Time.time;
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerInteractingDoor)]
    void OnInteractingDoor(PlayerInteractingDoorEventArgs ev)
    {
        if (ev.CanInteract)
            PickupValidator.DoorLastInteraction[ev.Door] = Time.time;
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerPickupItem)]
    void OnPickingUpItem(PlayerPickupItemEventArgs ev)
    {
        PickupValidator.OnPickingUpItem(ev);
    }
}