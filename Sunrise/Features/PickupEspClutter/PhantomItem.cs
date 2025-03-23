using System;
using Exiled.API.Enums;
using Exiled.API.Features.Pickups;
using JetBrains.Annotations;
using MEC;
using Mirror;
using NorthwoodLib.Pools;

namespace Sunrise.Features.PickupEspClutter;

public class PhantomItem : MonoBehaviour
{
    [UsedImplicitly] public static float VisibilityDistanceMultiplier = 1f;

    readonly HashSet<Player> _affectedPlayers = HashSetPool<Player>.Shared.Rent(30);
    ObjectDestroyMessage _destroyMessage;
    float _visibilityDistance;

    public static List<PhantomItem> List { get; } = [];

    void OnDestroy()
    {
        List.Remove(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (Player.Get(other) is Player player)
        {
            DestroyFor(player);
        }
    }

    IEnumerator<float> Initialize(Pickup pickup)
    {
        yield return Timing.WaitForOneFrame;

        const float VisibilityDistanceSurface = 70f;
        const float VisibilityDistanceFacility = 36f;

        _destroyMessage = new ObjectDestroyMessage { netId = pickup.Base.netIdentity.netId };
        _visibilityDistance = (pickup.Room is { Type: RoomType.Surface } ? VisibilityDistanceSurface : VisibilityDistanceFacility) * VisibilityDistanceMultiplier;

        gameObject.layer = (int)Layer.Trigger;
        var trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = _visibilityDistance;

        _affectedPlayers.UnionWith(Player.Dictionary.Values);
        _affectedPlayers.Remove(Server.Host);

        PhantomDestroy(pickup.Base.netIdentity);
        DestroyInProximity();

        List.Add(this);
    }

    void DestroyInProximity()
    {
        foreach (Player player in _affectedPlayers)
        {
            if (MathExtensions.SqrDistance(player.Position, transform.position) < _visibilityDistance)
                DestroyFor(player);
        }
    }

    void DestroyFor(Player player)
    {
        if (_affectedPlayers.Remove(player))
        {
            Log.Debug($"PhantomItem {_destroyMessage.netId} destroyed for {player.Nickname}");

            player.Connection.Send(_destroyMessage);

            if (_affectedPlayers.Count == 0)
            {
                Destroy(gameObject);
                Debug.Log($"PhantomItem {_destroyMessage.netId} destroyed for everyone. Remaining: {List.Count}");
            }
        }
    }

    public static void Create(ItemType type, Vector3 position)
    {
        var pickup = Pickup.CreateAndSpawn(type, position, Quaternion.identity);

        var gameObject = new GameObject($"PhantomItem-{type}");
        gameObject.transform.position = position;
        gameObject.AddComponent<PhantomItem>().Initialize(pickup).RunCoroutine();
    }

    public static void PhantomDestroy(NetworkIdentity identity)
    {
        if (NetworkServer.active)
        {
            if ((bool)(Object)NetworkServer.aoi)
            {
                try
                {
                    NetworkServer.aoi.OnDestroyed(identity);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        NetworkServer.spawned.Remove(identity.netId);
        identity.connectionToClient?.RemoveOwnedObject(identity);

        /*NetworkServer.SendToObservers<ObjectDestroyMessage>(identity, new ObjectDestroyMessage()
        {
            netId = identity.netId,
        });*/
        identity.ClearObservers();

        if (NetworkClient.active && NetworkServer.activeHost)
        {
            if (identity.isLocalPlayer)
                identity.OnStopLocalPlayer();
            identity.OnStopClient();
            identity.isOwned = false;
            identity.NotifyAuthority();
            NetworkClient.connection.owned.Remove(identity);
            NetworkClient.spawned.Remove(identity.netId);
        }

        identity.OnStopServer();
        identity.destroyCalled = true;

        if (Application.isPlaying)
            Destroy(identity.gameObject);
    }
}