using System.Linq;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Sunrise.Utility;

namespace Sunrise.Features.PickupValidation;

public class PickupValidationModule : PluginModule
{
    public const int ObstacleLayerMask = (int)Mask.HitregObstacles;

    protected override void OnEnabled()
    {
        Handlers.Player.PickingUpItem += OnPickingUpItem;
    }

    protected override void OnDisabled()
    {
        Handlers.Player.PickingUpItem -= OnPickingUpItem;
    }

    void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (!Config.Instance.PickupValidation || !ev.Player.IsConnected || !ev.Pickup.Base || ev.Player.IsNoclipPermitted)
            return;

        if (!CanPickUp(ev.Player, ev.Pickup))
            ev.IsAllowed = false;
    }

    static bool CanPickUp(Player player, Pickup pickup)
    {
        if (CanPickUpSimple(player, pickup))
            return true;

        Bounds bounds = pickup.Base.GetComponentInChildren<Collider>().bounds;
        Vector3 eyePos = player.CameraTransform.position;

        bool result = IsAccessibleFrom(eyePos, bounds);

        if (!result)
        {
            var upRay = new Ray(eyePos, Vector3.up);
            var jumpHeight = 0.5f;

            if (Physics.Raycast(upRay, out RaycastHit hit, jumpHeight, (int)Mask.PlayerObstacles))
                jumpHeight = hit.distance;

            eyePos += Vector3.up * jumpHeight;

            result = IsAccessibleFrom(eyePos, bounds);
        }

        if (!result && Config.Instance.DebugPrimitives)
        {
            Debug.DrawCube(bounds.center, bounds.size, Colors.Red, 10f);
            Debug.DrawPoint(eyePos, Colors.Green, 10f);

            foreach (Vector3 point in GetCorners(bounds))
            {
                Debug.DrawLine(eyePos, point, Colors.Red, 10f);
            }
        }

        return result;
    }

    static bool CanPickUpSimple(Player player, Pickup pickup) => !Physics.Linecast(player.CameraTransform.position, pickup.Position, ObstacleLayerMask);

    static bool IsAccessibleFrom(Vector3 point, Bounds bounds) => GetCorners(bounds).Any(corner => !Physics.Linecast(point, corner, out RaycastHit hit, ObstacleLayerMask) && !CanIgnoreHit(hit));

    static IEnumerable<Vector3> GetCorners(Bounds bounds)
    {
        Vector3 extents = bounds.extents;

        yield return bounds.center;

        yield return bounds.center + new Vector3(extents.x, extents.y, extents.z);
        yield return bounds.center + new Vector3(extents.x, extents.y, -extents.z);
        yield return bounds.center + new Vector3(extents.x, -extents.y, extents.z);
        yield return bounds.center + new Vector3(extents.x, -extents.y, -extents.z);

        yield return bounds.center + new Vector3(-extents.x, extents.y, extents.z);
        yield return bounds.center + new Vector3(-extents.x, extents.y, -extents.z);
        yield return bounds.center + new Vector3(-extents.x, -extents.y, extents.z);
        yield return bounds.center + new Vector3(-extents.x, -extents.y, -extents.z);
    }

    static bool CanIgnoreHit(RaycastHit hit) => hit.collider?.gameObject?.layer == (int)Layer.Doors && Door.Get(hit.collider.gameObject) is { IsKeycardDoor: false };
}