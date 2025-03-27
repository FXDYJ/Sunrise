using PlayerRoles.PlayableScps;

namespace Sunrise.Features.AntiWallhack;

/*
With 30 players randomly distributed in 30*30 area (approximate worst case) consumes around 130ms per 10 seconds
[02:24:46] [Sunrise] Time consumed by RaycastVisibilityChecker in last 10.01: 0.15 (1.45%)
[02:24:56] [Sunrise] Time consumed by RaycastVisibilityChecker in last 10.01: 0.13 (1.27%)
[02:25:06] [Sunrise] Time consumed by RaycastVisibilityChecker in last 10.01: 0.13 (1.27%)
[02:25:16] [Sunrise] Time consumed by RaycastVisibilityChecker in last 10.00: 0.12 (1.25%)
 */
internal static class RaycastVisibilityChecker
{
    static readonly Vector2[] VisibilityPointOffsets =
    [
        new(0, 0), // камера

        new(0.65f, -0.13f), // правое плечо (дальняя)
        new(-0.65f, -0.13f), // левое плечо (дальняя)

        new(0.3f, -1.03f), // правое колено (дальняя)
        new(-0.3f, -1.03f), // левое колено (дальняя)

        new(0f, 0.57f), // над головой

        new(0.27f, -1.43f), // около ног справа
        new(-0.27f, -1.43f), // около ног слева

        new(0.07f, -0.13f), // правое плечо
        new(-0.07f, -0.13f), // левое плечо

        new(0, -0.63f), // центр тела
    ];

    static readonly Vector3[] VisibilityPointsBuffer = new Vector3[VisibilityPointOffsets.Length];

    static readonly AutoBenchmark Benchmark = new("RaycastVisibilityChecker");

    public static bool IsVisible(Player playerA, Player playerB)
    {
        int key = RaycastVisibilityCache.GetKey(playerA.Id, playerB.Id);

        if (RaycastVisibilityCache.TryGet(key, out bool value))
            return value;

        Benchmark.Start();
        bool result = CheckAnyVisibility(playerA, playerB);
        RaycastVisibilityCache.Save(key, result);
        Benchmark.Increment();
        Benchmark.Stop();
        return result;
    }

    static bool CheckAnyVisibility(Player playerA, Player playerB) => CheckDirectionalVisibility(playerA, playerB) || CheckDirectionalVisibility(playerB, playerA);

    static bool CheckDirectionalVisibility(Player observer, Player target)
    {
        Vector3 observerPosition = observer.CameraTransform.position;
        Vector3 targetPosition = target.CameraTransform.position;

        Vector3 directionToObserver = (observerPosition - targetPosition).normalized;

        float widthMultiplier = Mathf.Clamp(target.Velocity.magnitude * 0.5f, 1f, 3.5f);
        SetVisibilityPoints(targetPosition, Vector3.Cross(directionToObserver, Vector3.up), Vector3.up, widthMultiplier);

        foreach (Vector3 point in VisibilityPointsBuffer)
        {
            if (Physics.Linecast(observerPosition, point, VisionInformation.VisionLayerMask))
            {
                Debug.DrawPoint(point, Colors.Orange * 50, 0.11f);
                continue;
            }

            Debug.DrawLine(observerPosition, point, Colors.Blue * 50, 0.11f);
            return true;
        }

        return false;
    }

    static void SetVisibilityPoints(Vector3 position, Vector3 right, Vector3 up, float widthMultiplier)
    {
        for (var i = 0; i < VisibilityPointOffsets.Length; i++)
        {
            Vector2 offset = VisibilityPointOffsets[i];
            VisibilityPointsBuffer[i] = position + offset.x * widthMultiplier * right + offset.y * up;
        }
    }
}

internal static class RaycastVisibilityCache
{
    const float NotVisibleCacheTime = 0.1f;
    const float VisibleCacheTime = 0.5f;

    static readonly Dictionary<int, (float Expiration, bool Value)> CachedVisibility = new();

    internal static void Save(int key, bool value)
    {
        float cacheTime = value ? VisibleCacheTime : NotVisibleCacheTime;
        CachedVisibility[key] = (Time.time + cacheTime, value);
    }

    internal static bool TryGet(int key, out bool value)
    {
        if (CachedVisibility.TryGetValue(key, out (float Expiration, bool Value) cache))
        {
            if (cache.Expiration - Time.time > 0)
            {
                value = cache.Value;
                return true;
            }
        }

        value = false;
        return false;
    }

    /// <summary>
    ///     Creates a unique order independent key for two player IDs.
    ///     Will start having collisions after 65535 players, which is way more than practical numbers.
    /// </summary>
    public static int GetKey(int a, int b)
    {
        if (a > b)
            return (a << 16) | b;
        else
            return (b << 16) | a;
    }
}