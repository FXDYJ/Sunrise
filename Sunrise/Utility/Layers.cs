using System;

namespace Sunrise.Utility;

public enum Layer
{
    DefaultColliders = 0,

    PlayerCollisionObject = 2,

    /// <summary>
    ///     This layer collides with players, but not pickups or other stuff, making it perfect for triggers
    /// </summary>
    Trigger = 4,

    /// <summary>
    ///     This layer is a hitreg target but doesn't collide with players. Depends on ShootingInteractions module
    /// </summary>
    NonCollidableHitreg = 6,

    PlayerObjects = 8,

    Pickups = 9,
    DoorButtons = 9,

    PlayerHitbox = 13,

    Glass = 14,

    InvisibleWalls = 16,

    Ragdolls = 17,

    Scp079CcTV = 18,
    DoorFrames = 18,

    ActiveGrenades = 20,

    Doors = 27,

    ShootThroughWalls = 29,
}

[Flags]
public enum Mask
{
    DefaultColliders = 1 << 0,

    PlayerCollisionObject = 1 << Layer.PlayerCollisionObject,

    NonCollidableHitreg = 1 << Layer.NonCollidableHitreg,

    PlayerObjects = 1 << Layer.PlayerObjects,

    Pickups = 1 << Layer.Pickups,
    DoorButtons = 1 << Layer.DoorButtons,

    PlayerHitbox = 1 << Layer.PlayerHitbox,

    Glass = 1 << Layer.Glass,

    InvisibleWalls = 1 << Layer.InvisibleWalls,

    Ragdolls = 1 << Layer.Ragdolls,

    Scp079CcTV = 1 << Layer.Scp079CcTV,
    DoorFrames = 1 << Layer.DoorFrames,

    ActiveGrenades = 1 << Layer.ActiveGrenades,

    Doors = 1 << Layer.Doors,

    ShootThroughWalls = 1 << Layer.ShootThroughWalls,

    HitregObstacles = Doors | DefaultColliders | Scp079CcTV | Glass,

    PlayerObstacles = HitregObstacles | InvisibleWalls | ShootThroughWalls,

    Hitreg = HitregObstacles | PlayerHitbox,
}