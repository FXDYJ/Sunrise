using System;
using AdminToys;
using PluginAPI.Core;
using PluginAPI.Events.Arguments;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;

namespace Sunrise.Features.PickupValidation;

internal static class PickupValidator
{
    internal static readonly Dictionary<Player, float> TemporaryPlayerBypass = new();
    internal static readonly Dictionary<object, float> LockerLastInteraction = new(); // Changed to object for LabAPI compatibility
    internal static readonly Dictionary<object, float> DoorLastInteraction = new(); // Changed to object for LabAPI compatibility

    static readonly RaycastHit[] HitBuffer = new RaycastHit[32];

    public static bool AlwaysBlock { get; set; }

    internal static void OnPickingUpItem(PlayerPickupItemEventArgs ev)
    {
        if (!Config.Instance.PickupValidation || ev.Item == null || ev.Player.IsNoclipEnabled)
            return;

        if (TemporaryPlayerBypass.TryGetValue(ev.Player, out float time) && time > Time.time)
            return;

        // Skip armor pickups
        if (ev.Item.ItemTypeId == ItemType.ArmorLight || ev.Item.ItemTypeId == ItemType.ArmorCombat || ev.Item.ItemTypeId == ItemType.ArmorHeavy)
            return;

        if (AlwaysBlock || !CanPickUp(ev.Player, ev.Item))
        {
            ev.CanPickup = false;
            if (Config.Instance.Debug)
                Log.Debug($"Pickup blocked for player {ev.Player.Nickname}: {ev.Item.ItemTypeId}");
        }
    }

    static bool CanPickUp(Player player, ItemPickupBase item)
    {
        Vector3 itemPosition = item.transform.position;
        float bypassTime = 1f; // Simplified bypass time calculation

        if (!IsObstructed(player.Camera.position, itemPosition, out _, bypassTime))
            return true;

        Bounds bounds = item.GetComponentInChildren<Renderer>()?.bounds ?? new Bounds(itemPosition, Vector3.one);
        Vector3 eyePos = player.Camera.position;
        Vector3 direction = player.Camera.forward;

        if (CanPickUpDirect(eyePos, direction, item))
            return true;

        bool result = CanPickupBounds(eyePos, bounds, bypassTime);

        if (!result)
        {
            Vector3 jumpEyePos = GetJumpEyePos(eyePos);

            result = CanPickupBounds(jumpEyePos, bounds, bypassTime);

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

    static bool CanPickUpDirect(Vector3 eyePos, Vector3 direction, ItemPickupBase item)
    {
        var ray = new Ray(eyePos, direction);

        int layerMask = LayerMask.GetMask("Default", "Pickup"); // Simplified layer mask
        if (!Physics.Raycast(ray, out RaycastHit pickupHit, 3, layerMask))
            return false;

        if (pickupHit.collider.gameObject.layer != LayerMask.NameToLayer("Pickup"))
            return false;

        if (pickupHit.collider.GetComponentInParent<ItemPickupBase>() != item)
            return false;

        Debug.DrawLine(ray.origin, pickupHit.point, Colors.Blue * 50, 15);
        return true;
    }

    static bool CanPickupBounds(Vector3 position, Bounds bounds, float bypassTime)
    {
        foreach (Vector3 corner in GetCorners(bounds))
        {
            if (IsObstructed(position, corner, out RaycastHit hit, bypassTime))
            {
                Debug.Log($"Hit {hit.collider?.gameObject?.name} ({hit.collider?.gameObject?.layer:G}) at {hit.point}");
                continue;
            }

            Debug.DrawLine(position, corner, Colors.Green * 50, 10f);
            return true;
        }

        return false;
    }

    static Vector3 GetJumpEyePos(Vector3 eyePos)
    {
        var upRay = new Ray(eyePos, Vector3.up);
        var jumpHeight = 0.75f;

        int layerMask = LayerMask.GetMask("Default", "Player"); // Simplified layer mask
        if (Physics.Raycast(upRay, out RaycastHit hit, jumpHeight, layerMask))
            jumpHeight = hit.distance - 0.05f;

        return upRay.GetPoint(jumpHeight);
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

    static bool IsObstructed(Vector3 a, Vector3 b, out RaycastHit hit, float bypassTime)
    {
        var ray = new Ray(a, b - a);
        int layerMask = LayerMask.GetMask("Default", "Door", "Glass"); // Simplified layer mask
        int count = Physics.RaycastNonAlloc(ray, HitBuffer, Vector3.Distance(a, b), layerMask);

        // Sort hits by distance
        Array.Sort(HitBuffer, 0, count, RaycastHitComparer.Instance);

        for (var i = 0; i < count; i++)
        {
            hit = HitBuffer[i];

            // Pass through recently interacted doors and lockers
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            
            if (layerName == "Door")
            {
                var door = hit.collider.gameObject.GetComponentInParent<DoorVariant>();
                if (door != null && DoorLastInteraction.TryGetValue(door, out float time) && time + bypassTime > Time.time)
                {
                    Debug.DrawPoint(hit.point, Colors.Green * 50, 10f);
                    continue;
                }
            }
            else if (layerName == "Glass")
            {
                var locker = hit.collider.GetComponentInParent<LockerChamber>();
                if (locker != null && LockerLastInteraction.TryGetValue(locker, out float time) && time + bypassTime > Time.time)
                {
                    Debug.DrawPoint(hit.point, Colors.Green * 50, 10f);
                    continue;
                }
            }

            // Check for primitives with collision disabled
            var toy = hit.collider.GetComponentInParent<PrimitiveObjectToy>();
            if (toy != null && !toy.NetworkPrimitiveFlags.HasFlag(PrimitiveFlags.Collidable))
            {
                Debug.DrawPoint(hit.point, Colors.Green * 50, 10f);
                continue;
            }

            // Obstruction found
            return true;
        }

        // No obstructions
        hit = default;
        return false;
    }

    class RaycastHitComparer : IComparer<RaycastHit>
    {
        public static readonly RaycastHitComparer Instance = new();
        public int Compare(RaycastHit hitA, RaycastHit hitB) => hitA.distance.CompareTo(hitB.distance);
    }
}