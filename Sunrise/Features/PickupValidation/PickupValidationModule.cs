using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using InventorySystem.Items.Usables.Scp1344;
using Sunrise.Utility;

namespace Sunrise.Features.PickupValidation;

public class PickupValidationModule : PluginModule
{
    public const int ObstacleLayerMask = (int)Mask.HitregObstacles;

    static readonly Dictionary<Player, float> TemporaryBypass = new();

    protected override void OnEnabled()
    {
        Handlers.Player.PickingUpItem += OnPickingUpItem;
        Handlers.Scp914.UpgradingInventoryItem += OnScp914UpgradingInventoryItem;
    }

    protected override void OnDisabled()
    {
        Handlers.Player.PickingUpItem -= OnPickingUpItem;
        Handlers.Scp914.UpgradingInventoryItem -= OnScp914UpgradingInventoryItem;
    }

    static void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
    {
        TemporaryBypass[ev.Player] = Time.time + 0.01f;
    }

    void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (!Config.Instance.PickupValidation || !ev.Player.IsConnected || !ev.Pickup.Base || ev.Player.Role is FpcRole { IsNoclipEnabled: true })
            return;

        if (TemporaryBypass.TryGetValue(ev.Player, out float time) && time > Time.time)
            return;

        if (!CanPickUp(ev.Player, ev.Pickup))
            ev.IsAllowed = false;
    }

    static bool CanPickUp(Player player, Pickup pickup)
    {
        if (CanPickUpSimple(player, pickup))
            return true;

        Bounds bounds = pickup.Base.GetComponentInChildren<Renderer>().bounds;

        Vector3 eyePos = player.CameraTransform.position;

        bool result = IsAccessibleFrom(eyePos, bounds);

        if (!result)
        {
            var upRay = new Ray(eyePos, Vector3.up);
            var jumpHeight = 0.5f;

            if (Physics.Raycast(upRay, out RaycastHit hit, jumpHeight, ObstacleLayerMask))
                jumpHeight = hit.distance - 0.05f;

            eyePos += Vector3.up * jumpHeight;

            result = IsAccessibleFrom(eyePos, bounds);

            if (!result && Config.Instance.DebugPrimitives)
            {
                foreach (Vector3 point in GetCorners(bounds))
                {
                    Debug.DrawLine(eyePos, point, Colors.Red * 30, 10f);
                }

                eyePos -= Vector3.up * jumpHeight;

                foreach (Vector3 point in GetCorners(bounds))
                {
                    Debug.DrawLine(eyePos, point, Colors.Red * 30, 10f);
                }
            }
        }

        Debug.DrawCube(bounds.center, bounds.size, result ? Colors.Green * 30 : Colors.Red * 30, 10f);

        return result;
    }

    static bool CanPickUpSimple(Player player, Pickup pickup) => !IsObstructed(player.CameraTransform.position, pickup.Position, out _);

    static bool IsAccessibleFrom(Vector3 camera, Bounds bounds)
    {
        foreach (Vector3 corner in GetCorners(bounds))
        {
            if (!IsObstructed(camera, corner, out RaycastHit hit) && !IsObstructed(corner, bounds.center, out _))
            {
                Debug.DrawLine(camera, corner, Colors.Green * 50, 10f);
                return true;
            }
            else
            {
                Debug.Log($"Hit {hit.collider?.gameObject?.name} ({hit.collider?.gameObject?.layer:G}) at {hit.point}");
            }
        }

        return false;
    }

    static bool IsObstructed(Vector3 a, Vector3 b, out RaycastHit hit) => Physics.Linecast(a, b, out hit, ObstacleLayerMask) && !CanIgnoreHit(hit);

    static bool CanIgnoreHit(RaycastHit hit) => hit.collider?.gameObject.layer == (int)Layer.Doors && Door.Get(hit.collider.gameObject) is { IsKeycardDoor: false };

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
}