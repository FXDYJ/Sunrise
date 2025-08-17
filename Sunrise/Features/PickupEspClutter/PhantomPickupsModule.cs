using PluginAPI.Events;
using PluginAPI.Events.Arguments;

namespace Sunrise.Features.PickupEspClutter;

internal class PhantomPickupsModule : PluginModule
{
    public static readonly List<ItemType> PhantomItemTypes =
    [
        ItemType.SCP500,
        ItemType.Adrenaline,
        ItemType.ParticleDisruptor,
        ItemType.MicroHID,
        ItemType.Jailbird,
        ItemType.SCP1344,
    ];

    protected override void OnEnabled()
    {
        EventManager.RegisterEvents<ServerEvents>(this);
        EventManager.RegisterEvents<PlayerEvents>(this);
    }

    protected override void OnDisabled()
    {
        EventManager.UnregisterEvents<ServerEvents>(this);
        EventManager.UnregisterEvents<PlayerEvents>(this);
    }

    protected override void OnReset()
    {
        PhantomPickup.Pickups.Clear();
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.RoundStart)]
    void OnRoundStarted()
    {
        PhantomItemSpawner.Start();
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.RoundEnd)]
    void OnRoundEnded(RoundEndEventArgs ev)
    {
        PhantomItemSpawner.Stop();
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerPickupItem)]
    void OnPickingUpItem(PlayerPickupItemEventArgs ev)
    {
        if (PhantomPickup.Pickups.Contains(ev.Item))
            ev.CanPickup = false;
    }
}