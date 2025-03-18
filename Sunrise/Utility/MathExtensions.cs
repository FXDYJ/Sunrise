namespace Sunrise.Utility;

public static class MathExtensions
{
    /// <summary>
    ///     Calculates the scalar parameter t that represents the closest point on the line segment defined by points a and b to the target point.
    /// </summary>
    public static float GetClosestT(Vector3 a, Vector3 b, Vector3 target)
    {
        if (a == b)
            return 0;

        Vector3 ab = b - a;
        Vector3 ac = target - a;

        float t = Vector3.Dot(ac, ab) / ab.sqrMagnitude;
        return Mathf.Clamp01(t);
    }

    public static float SqrDistance(Vector3 a, Vector3 b) => (a - b).sqrMagnitude;
}