using System.Diagnostics;
using MEC;

namespace Sunrise.Features.PickupEspClutter;

public static class PhantomItemSpawner
{
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
        const int CountPerRoom = 5;

        foreach (Room room in rooms)
        {
            for (var i = 0; i < CountPerRoom; i++)
            {
                PhantomItem.SpawnNew(room);
                yield return Timing.WaitForSeconds(Random.Range(0.05f, 0.1f));
            }
        }

        while (true)
        {
            rooms.ShuffleList();
            var sw = Stopwatch.StartNew();

            foreach (Room room in rooms)
            {
                sw.Start();
                PhantomItem.DestroyOldest();
                PhantomItem.SpawnNew(room);
                sw.Stop();

                yield return Timing.WaitForSeconds(Random.Range(0.1f, 0.3f));
            }
            
            Log.Warn($"Phantom item destruction and spawn for {rooms.Count} rooms took {sw.Elapsed.TotalMilliseconds:F7}ms");

            yield return Timing.WaitForSeconds(0.5f);
        }
    }
}