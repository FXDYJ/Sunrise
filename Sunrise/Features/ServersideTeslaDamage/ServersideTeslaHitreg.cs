using MEC;
using Sunrise.Utility;

namespace Sunrise.Features.ServersideTeslaDamage;

public class ServersideTeslaHitreg(TeslaGate tesla)
{
    // The amount of time a player has to be inside a bursting tesla gate to be considered hit
    public static readonly float HitThreshold = 0.2f;
    public static readonly float ShockDuration = 0.5f;

    readonly Collider[] _hitBuffer = new Collider[32];
    readonly HashSet<Player> _distinctHitPlayers = [];

    public readonly Bounds Bounds = new(tesla.transform.position + Vector3.up * (tesla.sizeOfKiller.y / 2f), tesla.transform.rotation * new Vector3(4f, 0.7f, 5f));
    public readonly Dictionary<Player, float> HitTimers = new();

    public void Burst() => Timing.RunCoroutine(BurstCoroutine());

    IEnumerator<float> BurstCoroutine()
    {
        float remaining = ShockDuration;

        Debug.DrawCube(Bounds.center, Bounds.size, Colors.Yellow * 30, ShockDuration);

        while (remaining > 0)
        {
            UpdateBurst();
            remaining -= Timing.DeltaTime;
            yield return Timing.WaitForOneFrame;
        }

        ProcessHits();
    }

    void UpdateBurst()
    {
        int count = Physics.OverlapBoxNonAlloc(Bounds.center, Bounds.extents, _hitBuffer, Quaternion.identity, (int)Mask.PlayerHitbox);

        for (var i = 0; i < count; i++)
        {
            Collider collider = _hitBuffer[i];

            if (Player.Get(collider) is Player player)
                _distinctHitPlayers.Add(player);
        }

        foreach (Player player in _distinctHitPlayers)
        {
            HitTimers.TryAdd(player, 0);
            HitTimers[player] += Timing.DeltaTime;
        }

        _distinctHitPlayers.Clear();
    }

    void ProcessHits()
    {
        foreach ((Player player, float time) in HitTimers)
        {
            if (time < HitThreshold)
                continue;

            // Players who sent the message themselves should not be shocked again
            if (ServersideTeslaDamageModule.ShockedPlayers.TryGetValue(player, out float shockTime) && Time.time - shockTime < ShockDuration * 2)
                continue;

            TeslaGateController.ServerReceiveMessage(player.Connection, new(tesla));
        }

        HitTimers.Clear();
    }
}