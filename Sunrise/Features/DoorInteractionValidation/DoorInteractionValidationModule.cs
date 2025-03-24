using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;

namespace Sunrise.Features.DoorInteractionValidation;

public class AntiDoorManipulatorModule : PluginModule
{
    protected override void OnEnabled()
    {
        Handlers.Player.InteractingDoor += OnInteractingDoor;
    }

    protected override void OnDisabled()
    {
        Handlers.Player.InteractingDoor -= OnInteractingDoor;
    }

    static void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        if (!Config.Instance.DoorInteractionValidation || ev.Player.Role is FpcRole { IsNoclipEnabled: true })
            return;

        if (ev is not { Door: not null, Collider: not null, Player: not null })
            return;

        if (!CanInteract(ev.Player, ev))
        {
            if (Config.Instance.Debug)
                ev.IsAllowed = false;
            else
                ev.CanInteract = false;
        }
    }

    static bool CanInteract(Player player, InteractingDoorEventArgs ev)
    {
        Vector3 colliderPos = ev.Collider.transform.position + ev.Collider.transform.TransformDirection(ev.Collider.VerificationOffset);

        if (CanInteractWithCollider(player, colliderPos))
            return true;

        foreach (BoxCollider collider in ev.Door.Base._colliders)
        {
            if (CanInteractWithCollider(player, collider.transform.position + collider.transform.TransformDirection(collider.center)))
                return true;
        }

        Ray ray = new(player.CameraTransform.position, player.CameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 3, (int)(Mask.Doors | Mask.DoorButtons | Mask.Glass)))
        {
            Debug.DrawLine(ray.origin, hit.point, Colors.Yellow * 50, 15);
            return true;
        }

        Debug.Log($"Door interaction blocked. Player: {player.Nickname}, Door: {ev.Door.Position}, Collider: {colliderPos}");
        return false;
    }

    static bool CanInteractWithCollider(Player player, Vector3 colliderPos)
    {
        Vector3 direction = (colliderPos - player.CameraTransform.position).normalized;
        float angle = Vector3.Angle(player.CameraTransform.forward with { y = direction.y }, direction);

        if (angle < 45)
            return true;

        return false;
    }
}