using Exiled.Events.EventArgs.Server;
using PlayerRoles.FirstPersonControl;

namespace Sunrise.Features.PickupEspClutter;

public class PhantomPickupsModule : PluginModule
{
    public static readonly List<ItemType> PhantomItemTypes =
    [
        ItemType.SCP500,
        ItemType.Adrenaline,
        ItemType.KeycardO5,
        ItemType.KeycardFacilityManager,
        ItemType.KeycardChaosInsurgency,
    ];

    protected override void OnEnabled()
    {
        return; // BUG: Triggers collide with players pushing them off the map

        if (!Config.Instance.PhantomPickups)
            return;

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