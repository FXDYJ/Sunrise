using System.Diagnostics;
using AdminToys;
using MEC;
using Mirror;

namespace Sunrise.Utility;

internal static class Debug
{
    [Conditional("DEBUG")]
    internal static void DrawCube(Vector3 position, Vector3 scale, Color color = default, float duration = 10f)
    {
        if (!Config.Instance.DebugPrimitives)
            return;

        // Simplified approach - just log for now since primitive creation is complex in LabAPI
        Log($"DrawCube at {position} with scale {scale} and color {color}");
    }

    [Conditional("DEBUG")]
    internal static void DrawLine(Vector3 start, Vector3 end, Color color = default, float duration = 10f)
    {
        if (!Config.Instance.DebugPrimitives)
            return;

        // Simplified approach - just log for now
        Log($"DrawLine from {start} to {end} with color {color}");
    }

    [Conditional("DEBUG")]
    internal static void DrawPoint(Vector3 position, Color color = default, float duration = 10f)
    {
        if (!Config.Instance.DebugPrimitives)
            return;

        // Simplified approach - just log for now
        Log($"DrawPoint at {position} with color {color}");
    }

    [Conditional("DEBUG")]
    internal static void Log(string s)
    {
        PluginAPI.Core.Log.Debug(s);
    }

    static Color GetColor(Color color)
    {
        if (color == default)
            color = Color.red;

        if (color.a > 1)
            color.a = 0.1f;

        return color;
    }
}