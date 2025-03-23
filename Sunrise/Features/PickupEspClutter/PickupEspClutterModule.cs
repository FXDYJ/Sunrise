using System;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items.Usables;
using MEC;

namespace Sunrise.Features.PickupEspClutter;

public class PickupEspClutterModule : PluginModule
{
    protected override void OnEnabled()
    {
        Handlers.Server.RoundStarted += OnRoundStarted;
        Handlers.Server.RoundEnded += OnRoundEnded;
    }

    protected override void OnDisabled()
    {
        Handlers.Server.RoundStarted -= OnRoundStarted;
        Handlers.Server.RoundEnded -= OnRoundEnded;
    }

    static void OnRoundStarted()
    {
        PhantomItemSpawner.Start();
    }

    static void OnRoundEnded(RoundEndedEventArgs ev)
    {
        PhantomItemSpawner.Stop();
    }
}

public static class PhantomItemSpawner
{
    static readonly List<ItemType> PhantomItemTypes =
    [
        ItemType.SCP500,
    ];

    static CoroutineHandle coroutineHandle;

    public static void Start()
    {
        Timing.KillCoroutines(coroutineHandle);
        coroutineHandle = Timing.RunCoroutine(PhantomItemSpawnerCoroutine());
    }

    public static void Stop()
    {
        Timing.KillCoroutines(coroutineHandle);
    }

    static IEnumerator<float> PhantomItemSpawnerCoroutine()
    {
        List<Room> rooms = new(Room.List);
        const int MaxCount = 300;
        int intialCount = MaxCount;

        while (true)
        {
            rooms.ShuffleList();

            foreach (Room room in rooms)
            {
                const float RandomOffset = 5f;
                Vector3 position = room.Position + (Random.insideUnitSphere * RandomOffset) with { y = 0.5f };
                ItemType itemType = PhantomItemTypes.RandomItem();

                PhantomItem.Create(itemType, position);

                if (intialCount-- > 0)
                    continue;

                while (PhantomItem.List.Count >= MaxCount)
                    yield return Timing.WaitForSeconds(1f);

                yield return Timing.WaitForSeconds(1f + Random.value);
            }

            yield return Timing.WaitForSeconds(1f + Random.value);
        }
    }
}