using MEC;

namespace Sunrise.Features.PickupEspClutter;

public static class PhantomItemSpawner
{
    static readonly List<ItemType> PhantomItemTypes =
    [
        ItemType.SCP500,
        ItemType.Adrenaline,
        ItemType.KeycardO5,
        ItemType.KeycardFacilityManager,
        ItemType.KeycardChaosInsurgency,
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
                SpawnNew(room);

                if (intialCount-- > 0)
                    continue;

                while (PhantomItem.List.Count >= MaxCount)
                    yield return Timing.WaitForSeconds(1f);

                yield return Timing.WaitForSeconds(Random.Range(0.1f, 0.3f));
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }

    static void SpawnNew(Room room)
    {
        const float RandomOffset = 5f;
        Vector3 position = room.Position + (Random.insideUnitSphere * RandomOffset) with { y = 0.5f };
        ItemType itemType = PhantomItemTypes.RandomItem();

        PhantomItem.Create(itemType, position);
    }
}