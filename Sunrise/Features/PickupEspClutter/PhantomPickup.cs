using InventorySystem.Items.Pickups;
using JetBrains.Annotations;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using PluginAPI.Core;
using Sunrise.API.Visibility;

namespace Sunrise.Features.PickupEspClutter;

internal class PhantomPickup : MonoBehaviour
{
    readonly HashSet<Player> _hiddenFor = HashSetPool<Player>.Shared.Rent();
    CoroutineHandle _coroutine;
    NetworkIdentity _netIdentity = null!;

    LinkedListNode<PhantomPickup> _node = null!;
    ItemPickupBase _pickup = null!;

    [UsedImplicitly] public static bool DebugMode { get; set; }

    internal static LinkedList<PhantomPickup> List { get; } = [];
    internal static HashSet<ItemPickupBase> Pickups { get; } = [];

    void Start()
    {
        _node = List.AddLast(this);
        Pickups.Add(_pickup);

        _pickup = gameObject.GetComponent<ItemPickupBase>();
        _netIdentity = _pickup.netIdentity;

        _coroutine = Timing.RunCoroutine(Coroutine());
    }

    void OnDestroy()
    {
        List.Remove(_node);
        Pickups.Remove(_pickup);

        if (_pickup != null)
            NetworkServer.Destroy(_pickup.gameObject);
        HashSetPool<Player>.Shared.Return(_hiddenFor);

        Timing.KillCoroutines(_coroutine);
    }

    public void Destroy()
    {
        if (_pickup != null)
            NetworkServer.Destroy(_pickup.gameObject);
    }

    IEnumerator<float> Coroutine()
    {
        while (true)
        {
            HideForEveryone();

            yield return Timing.WaitForSeconds(Random.Range(0.4f, 0.5f));

            // Choose a new position for the item
            PhantomPickupSynchronizer.GetNextPosition(out Vector3 position);
            _pickup.transform.position = position;

            // Wait for the item to change position to one where it wont be noticed by legit players immediately, so we can safely update visibility
            yield return Timing.WaitForSeconds(Random.Range(0.4f, 0.5f));

            // Lay on the ground for some time
            float idleTime = Random.Range(5f, 15f);

            while (idleTime > 0)
            {
                // Update visibility for all players
                UpdateVisibility();

                float waitTime = Random.Range(1f, 2f);
                idleTime -= waitTime;

                yield return Timing.WaitForSeconds(waitTime);
            }
        }
    }

    void HideForEveryone()
    {
        foreach (Player player in Player.GetPlayers())
            SetVisibility(player, false);
    }

    void UpdateVisibility()
    {
        VisibilityData? visibilityData = VisibilityData.Get(transform.position, false);

        if (visibilityData == null)
        {
            HideForEveryone();
            return;
        }

        foreach (Player player in Player.GetPlayers())
            SetVisibility(player, !IsObserving(player, visibilityData));
    }

    void SetVisibility(Player player, bool visibility)
    {
        switch (visibility)
        {
            case true when _hiddenFor.Remove(player):
                NetworkServer.ShowForConnection(_netIdentity, player.ReferenceHub.connectionToClient);
                break;
            case false when _hiddenFor.Add(player):
                NetworkServer.HideForConnection(_netIdentity, player.ReferenceHub.connectionToClient);
                break;
        }
    }

    // Whether the player will be able to see the item
    static bool IsObserving(Player player, VisibilityData visibilityData)
    {
        if (DebugMode)
            return MathExtensions.SqrDistance(player.Position, visibilityData.TargetRoom.Position) < 1.5f * 1.5f;

        return visibilityData.IsVisible(player);
    }

    internal static void DestroyOldest()
    {
        if (List.Count > 0)
            List.First.Value.Destroy();
    }

    internal static void Create()
    {
        PhantomPickupSynchronizer.GetNextPosition(out Vector3 position);
        ItemType type = PhantomPickupsModule.PhantomItemTypes.RandomItem();
        
        // Simplified pickup creation for LabAPI
        var pickupPrefab = NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.GetComponent<ItemPickupBase>()?.ItemTypeId == type);
        if (pickupPrefab != null)
        {
            var pickup = UnityEngine.Object.Instantiate(pickupPrefab, position, Random.rotation);
            pickup.AddComponent<PhantomPickup>();
            NetworkServer.Spawn(pickup);
        }
    }
}