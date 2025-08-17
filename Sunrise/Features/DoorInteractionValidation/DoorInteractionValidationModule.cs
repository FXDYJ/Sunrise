using PluginAPI.Core;
using PluginAPI.Events;
using PluginAPI.Events.Arguments;

namespace Sunrise.Features.DoorInteractionValidation;

internal class AntiDoorManipulatorModule : PluginModule
{
    protected override void OnEnabled()
    {
        EventManager.RegisterEvents<PlayerEvents>(this);
    }

    protected override void OnDisabled()
    {
        EventManager.UnregisterEvents<PlayerEvents>(this);
    }

    [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerInteractingDoor)]
    void OnInteractingDoor(PlayerInteractingDoorEventArgs ev)
    {
        if (!Config.Instance.DoorInteractionValidation || ev.Player.IsNoclipEnabled)
            return;

        if (ev.Door == null || ev.Player == null)
            return;

        if (!CanInteract(ev.Player, ev))
        {
            ev.CanInteract = false;
            if (Config.Instance.Debug)
                Log.Debug($"Door interaction blocked for player {ev.Player.Nickname}");
        }
    }

    static bool CanInteract(Player player, PlayerInteractingDoorEventArgs ev)
    {
        // Check if player is looking at the door
        Vector3 doorPosition = ev.Door.Position;
        
        if (LooksAtDoor(player, doorPosition))
            return true;

        // Raycast check for door interaction
        Ray ray = new(player.Camera.position, player.Camera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 3, LayerMask.GetMask("Door", "DoorButton", "Glass")))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.yellow, 15);
            return true;
        }

        Log.Debug($"Door interaction blocked. Player: {player.Nickname}, Door: {ev.Door.Position}");
        return false;
    }

    static bool LooksAtDoor(Player player, Vector3 doorPos)
    {
        const float AllowedAngle = 30;

        Vector3 direction = (doorPos - player.Camera.position).normalized;
        float angle = Vector3.Angle(player.Camera.forward, direction);

        return angle < AllowedAngle;
    }
}