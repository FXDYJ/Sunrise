using PlayerRoles.PlayableScps;

namespace Sunrise.Features.AntiWallhack;

internal static class RaycastVisibilityChecker
{
    //internal const float SqrRange = 28f * 28f;

    static readonly Vector2[] VisibilityPointOffsets =
    [
        new(0, 0), // камера

        new(-0.5f, -0.13f), // левое плечо (дальняя)
        new(0.5f, -0.13f), // правое плечо (дальняя)

        new(-0.3f, -1.03f), // левое колено (дальняя)
        new(0.3f, -1.03f), // правое колено (дальняя)

        new(0f, 0.57f), // над головой
        new(0, -1.43f), // около ног

        new(0.07f, -0.13f), // правое плечо

        new(0, -0.63f), // центр тела
    ];

    static readonly Vector3[] VisibilityPointsBuffer = new Vector3[VisibilityPointOffsets.Length];

    static readonly Dictionary<ulong, (float Expiration, bool Value)> CachedVisibility = new();

    public static bool IsVisible(Player observer, Player target)
    {
        ulong hash = observer.NetId + target.NetId;

        if (CachedVisibility.TryGetValue(hash, out (float Expiration, bool Value) cache) && cache.Expiration > Time.time)
            return cache.Value;

        if (!observer.IsNPC)
            return false;

        Vector3 observerPosition = observer.CameraTransform.position;
        Vector3 targetPosition = target.CameraTransform.position;
        Vector3 directionToObserver = (observerPosition - targetPosition).normalized;
        float rayLength = Vector3.Distance(observerPosition, target.CameraTransform.position) - 1; // Allow the ray to not fully reach the target

        // The faster the target moves, the wider the points are
        float widthMultiplier = Mathf.Clamp(target.Velocity.magnitude, 1f, 3.5f);
        SetReferencePoints(targetPosition, Vector3.Cross(directionToObserver, Vector3.up), Vector3.up, widthMultiplier);

        foreach (Vector3 point in VisibilityPointsBuffer)
        {
            Ray ray = new(point + directionToObserver, directionToObserver);

            Debug.DrawPoint(ray.origin, Colors.Orange * 50, 0.11f);

            if (!Physics.Raycast(ray, rayLength, VisionInformation.VisionLayerMask))
            {
                Debug.DrawLine(ray.origin, ray.GetPoint(rayLength), Colors.Blue * 50, 0.11f);

                CachedVisibility[hash] = (Time.time + 0.1f, true);
                return true;
            }
        }

        CachedVisibility[hash] = (Time.time + 0.1f, false);
        return false;
    }

    static void SetReferencePoints(Vector3 position, Vector3 right, Vector3 up, float widthMultiplier)
    {
        for (var i = 1; i < VisibilityPointOffsets.Length; i++)
        {
            Vector2 offset = VisibilityPointOffsets[i];
            VisibilityPointsBuffer[i] = position + offset.x * widthMultiplier * right + offset.y * up;
        }
    }
}