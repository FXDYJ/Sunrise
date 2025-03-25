namespace Sunrise.API.Backtracking;

/// <summary>
///     A single entry in the serverside backtrack history.
/// </summary>
internal struct BacktrackEntry
{
    internal BacktrackEntry(Player player)
    {
        Position = player.Position;
        Rotation = player.CameraTransform.rotation;
    }

    /// <summary>
    ///     A single entry in the serverside backtrack history.
    /// </summary>
    internal BacktrackEntry(Vector3 position, Quaternion rotation)
    {
        Rotation = rotation;
        Position = position;
    }

    internal Quaternion Rotation { get; set; }
    internal Vector3 Position { get; set; }
    internal float Timestamp { get; set; } = Time.time;

    internal readonly float Age => Time.time - Timestamp;

    internal readonly void Restore(Player player)
    {
        player.Transform.position = Position;
        player.CameraTransform.rotation = Rotation;
    }
}