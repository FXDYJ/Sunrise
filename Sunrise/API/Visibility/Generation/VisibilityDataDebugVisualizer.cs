using MapGeneration;

namespace Sunrise.API.Visibility.Generation;

internal static class VisibilityDataDebugVisualizer
{
    const float PrimitiveDuration = 15;

    static readonly Dictionary<Vector3Int, DebugData> RoomDebugData = new();

    public static void DrawDebugPrimitives(VisibilityData data)
    {
        Vector3Int coords = RoomIdUtils.PositionToCoords(data.Room.Position);

        if (!RoomDebugData.TryGetValue(coords, out DebugData debugData))
        {
            debugData = new(coords);
            RoomDebugData[coords] = debugData;
        }

        if (!debugData.IsReady())
            return;

        debugData.ResetTime();
        VisualizeRoomConnections(data, debugData);
        VisualizeRoomAxes(data, debugData.Origin);
    }

    static void VisualizeRoomConnections(VisibilityData data, DebugData debugData)
    {
        Debug.DrawPoint(debugData.Origin, debugData.Color, PrimitiveDuration);

        foreach (Vector3Int coord in data.VisibleCoords)
        {
            Debug.DrawLine(debugData.Origin, RoomIdUtils.CoordsToCenterPos(coord) + debugData.OriginOffset, debugData.Color, PrimitiveDuration);
        }
    }

    static void VisualizeRoomAxes(VisibilityData data, Vector3 origin)
    {
        Transform transform = data.Room.transform;
        origin += Vector3.up * 0.05f;

        Debug.DrawLine(origin, origin + transform.right * 0.3f, Colors.Red * 50, PrimitiveDuration);
        Debug.DrawLine(origin, origin + transform.forward * 0.3f, Colors.Blue * 50, PrimitiveDuration);
        Debug.DrawLine(origin, origin + transform.up * 0.3f, Colors.Green * 50, PrimitiveDuration);
    }

    struct DebugData(Vector3Int coords)
    {
        public readonly Color Color = GetRandomColor();
        public readonly Vector3 OriginOffset = GetOriginOffset();
        public readonly Vector3 Position = RoomIdUtils.CoordsToCenterPos(coords);
        public readonly Vector3 Origin => Position + OriginOffset;

        float _lastPrimitiveSpawnTime = Time.time;

        static Color GetRandomColor() =>
            Random.ColorHSV(0, 1, 0.7f, 1, 0.7f, 1) * 50;

        static Vector3 GetOriginOffset() =>
            (Random.insideUnitSphere * 0.5f) with { y = Random.Range(0, 0.2f) } +
            Vector3.up * 0.3f;

        public void ResetTime() => _lastPrimitiveSpawnTime = Time.time;
        public bool IsReady() => Time.time > _lastPrimitiveSpawnTime + PrimitiveDuration;
    }
}