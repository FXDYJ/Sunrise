using Exiled.API.Extensions;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using InventorySystem.Items.Pickups;

namespace Sunrise.Features.PickupValidation;

public class PickupValidationModule : PluginModule
{
    static readonly Dictionary<Player, float> TemporaryPlayerBypass = new();

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
        TemporaryPlayerBypass[ev.Player] = Time.time + 0.01f;
    }

    void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (!Config.Instance.PickupValidation || !ev.Pickup.Base || ev.Player.Role is FpcRole { IsNoclipEnabled: true })
            return;

        if (TemporaryPlayerBypass.TryGetValue(ev.Player, out float time) && time > Time.time)
            return;

        // Exiled bug - armor pickups become permanently locked if the event is denied
        if (ev.Pickup is BodyArmorPickup)
            return;

        if (!CanPickUp(ev.Player, ev.Pickup))
            ev.IsAllowed = false;
    }

    static bool CanPickUp(Player player, Pickup pickup)
    {
        if (!IsObstructed(player.CameraTransform.position, pickup.Position, out _))
            return true;

        Bounds bounds = pickup.Base.GetComponentInChildren<Renderer>().bounds;
        Vector3 eyePos = player.CameraTransform.position;
        Vector3 direction = player.CameraTransform.forward;

        if (CanPickUpDirect(eyePos, direction, pickup))
            return true;

        bool result = CanPickupBounds(eyePos, bounds);

        if (!result)
        {
            Vector3 jumpEyePos = GetJumpEyePos(eyePos);

            result = CanPickupBounds(jumpEyePos, bounds) || CanPickUpDirect(jumpEyePos, direction, pickup);

            if (!result && Config.Instance.DebugPrimitives)
            {
                foreach (Vector3 point in GetCorners(bounds))
                {
                    Debug.DrawLine(eyePos, point, Colors.Red * 30, 10f);
                }

                foreach (Vector3 point in GetCorners(bounds))
                {
                    Debug.DrawLine(jumpEyePos, point, Colors.Red * 30, 10f);
                }
            }
        }

        Debug.DrawCube(bounds.center, bounds.size, result ? Colors.Green * 30 : Colors.Red * 30, 10f);

        return result;
    }

    static bool CanPickUpDirect(Vector3 eyePos, Vector3 direction, Pickup pickup)
    {
        var ray = new Ray(eyePos, direction);

        if (!Physics.Raycast(ray, out RaycastHit pickupHit, 3, (int)(Mask.HitregObstacles | Mask.Pickups)))
            return false;

        if (pickupHit.collider.gameObject.layer != (int)Layer.Pickups)
            return false;

        if (pickupHit.collider.GetComponentInParent<ItemPickupBase>() != pickup.Base)
            return false;

        Debug.DrawLine(ray.origin, pickupHit.point, Colors.Blue * 50, 15);
        return true;
    }

    static bool CanPickupBounds(Vector3 position, Bounds bounds)
    {
        foreach (Vector3 corner in GetCorners(bounds))
        {
            if (!IsObstructed(position, corner, out RaycastHit hit))
            {
                Debug.DrawLine(position, corner, Colors.Green * 50, 10f);
                return true;
            }
            else
            {
                Debug.Log($"Hit {hit.collider?.gameObject?.name} ({hit.collider?.gameObject?.layer:G}) at {hit.point}");
            }
        }

        return false;
    }

    static Vector3 GetJumpEyePos(Vector3 eyePos)
    {
        var upRay = new Ray(eyePos, Vector3.up);
        var jumpHeight = 0.75f;

        if (Physics.Raycast(upRay, out RaycastHit hit, jumpHeight, (int)Mask.PlayerObstacles))
            jumpHeight = hit.distance - 0.05f;

        return upRay.GetPoint(jumpHeight);
    }

    static bool IsObstructed(Vector3 a, Vector3 b, out RaycastHit hit) => Physics.Linecast(a, b, out hit, (int)Mask.HitregObstacles) && !CanIgnoreHit(hit);

    static bool CanIgnoreHit(RaycastHit hit)
    {
        // Non-keycard doors
        int layer = hit.collider.gameObject.layer;

        if (layer == (int)Layer.Doors && Door.Get(hit.collider.gameObject) is { IsKeycardDoor: false })
            return true;

        /*if (layer == (int)Layer.Glass && hit.collider.TryGetComponent<PedestalScpLocker>(out PedestalScpLocker? locker) && locker.)*/ //BUG: PedestalScpLocker doors can block legit pickups after moving
        return false;
    }

    static IEnumerable<Vector3> GetCorners(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        yield return center;

        yield return center + new Vector3(extents.x, extents.y, extents.z);
        yield return center + new Vector3(extents.x, extents.y, -extents.z);
        yield return center + new Vector3(extents.x, -extents.y, extents.z);
        yield return center + new Vector3(extents.x, -extents.y, -extents.z);

        yield return center + new Vector3(-extents.x, extents.y, extents.z);
        yield return center + new Vector3(-extents.x, extents.y, -extents.z);
        yield return center + new Vector3(-extents.x, -extents.y, extents.z);
        yield return center + new Vector3(-extents.x, -extents.y, -extents.z);
    }
}