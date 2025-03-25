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
        const int Count = 100;
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < Count; i++)
        {
            sw.Start();
            PhantomPickup.Create();
            sw.Stop();
            yield return Timing.WaitForOneFrame;
        }

        sw.Stop();
        Debug.Log($"Spawned {Count} phantom pickups in {sw.Elapsed.TotalMilliseconds:F5}ms");
        

        while (true)
        {
            PhantomPickup.DestroyOldest();
            yield return Timing.WaitForSeconds(Random.Range(1, 2f));
            PhantomPickup.Create();
            yield return Timing.WaitForSeconds(Random.Range(1, 2f));
        }
    }
}