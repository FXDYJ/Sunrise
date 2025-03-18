using System.Diagnostics;
using AdminToys;
using Exiled.API.Features.Toys;
using MEC;
using Sunrise.EntryPoint;

namespace Sunrise.Utility;

public static class Debug
{
    [Conditional("DEBUG")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color = default, float duration = 10f)
    {
        if (!SunrisePlugin.Instance.Config.DebugPrimitives)
            return;

        color = GetColor(color);

        GetLineData(start, end, 0.01f, false, out Vector3 position, out Vector3 scale, out Quaternion rotation);
        var line = Primitive.Create(PrimitiveType.Cylinder, PrimitiveFlags.Visible, position, rotation.eulerAngles, scale, true, color);
        Timing.CallDelayed(duration, line.Destroy);
    }

    [Conditional("DEBUG")]
    public static void DrawPoint(Vector3 position, Color color = default, float duration = 10f)
    {
        if (!SunrisePlugin.Instance.Config.DebugPrimitives)
            return;

        color = GetColor(color);

        var point = Primitive.Create(PrimitiveType.Sphere, PrimitiveFlags.Visible, position, Vector3.zero, Vector3.one * 0.1f, true, color);
        Timing.CallDelayed(duration, point.Destroy);
    }

    public static void GetLineData(Vector3 from, Vector3 to, float thickness, bool cube, out Vector3 position, out Vector3 scale, out Quaternion rotation)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        scale = new Vector3(thickness, distance * (cube ? 1 : 0.5f), thickness);
        position = from + direction * 0.5f;
        rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
    }

    static Color GetColor(Color color)
    {
        if (color == default)
            color = Color.red;

        if (color.a > 1)
            color.a = 0.1f;

        return color;
    }

    [Conditional("DEBUG")]
    public static void Log(string s)
    {
        Exiled.API.Features.Log.Debug(s);
    }
}