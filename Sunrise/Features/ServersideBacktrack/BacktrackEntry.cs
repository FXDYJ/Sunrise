namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     A single entry in the serverside backtrack history.
/// </summary>
public struct BacktrackEntry(Vector3 position, Quaternion rotation)
{
    public BacktrackEntry(Player player) : this(player.Position, player.CameraTransform.rotation) { }

    public Quaternion Rotation { get; set; } = rotation;
    public Vector3 Position { get; set; } = position;
    public float Timestamp { get; set; } = Time.time;

    public readonly float Age => Time.time - Timestamp;

    public readonly void Restore(Player player)
    {
        player.Transform.position = Position;
        player.CameraTransform.rotation = Rotation;
    }
}