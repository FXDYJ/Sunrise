using Exiled.API.Features.Roles;
using PlayerRoles.FirstPersonControl;

namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     A single entry in the serverside backtrack history.
/// </summary>
public struct BacktrackEntry
{
    public BacktrackEntry(Player player)
    {
        Position = player.Position;

        if (player.Role is not FpcRole fpcRole)
        {
            Log.Warn($"Player {player.Nickname} is not a FpcRole");
            Rotation = player.CameraTransform.rotation;
            return;
        }

        FpcMouseLook mouseLook = fpcRole.FirstPersonController.FpcModule.MouseLook;
        /*Rotation = Quaternion.Euler(mouseLook._prevSyncV, mouseLook._prevSyncH, 0);*/
        Rotation = ConvertToQuaternion(mouseLook._prevSyncH, mouseLook._prevSyncV);
    }

    /// <summary>
    ///     A single entry in the serverside backtrack history.
    /// </summary>
    public BacktrackEntry(Vector3 position, Quaternion rotation)
    {
        Rotation = rotation;
        Position = position;
    }

    public Quaternion Rotation { get; set; }
    public Vector3 Position { get; set; }
    public float Timestamp { get; set; } = Time.time;

    public readonly float Age => Time.time - Timestamp;

    public readonly void Restore(Player player)
    {
        player.Transform.position = Position;
        player.CameraTransform.rotation = Rotation;
    }

    public static Quaternion ConvertToQuaternion(ushort horizontal, ushort vertical)
    {
        float hAngle = Mathf.Lerp(0f, 360, (float)horizontal / ushort.MaxValue);
        float vAngle = Mathf.Lerp(-88, 88, (float)vertical / ushort.MaxValue);

        Quaternion hRot = Quaternion.Euler(Vector3.up * hAngle).normalized;
        Quaternion vRot = Quaternion.Euler(Vector3.left * vAngle).normalized;

        return (hRot * vRot).normalized;
    }
}