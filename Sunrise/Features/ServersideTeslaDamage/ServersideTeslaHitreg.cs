using MEC;
using Sunrise.Utility;

namespace Sunrise.Features.ServersideTeslaDamage;

public class ServersideTeslaHitreg(TeslaGate tesla)
{
    // The amount of time the tesla has to be bursting before dealing damage on server side
    public static readonly float ClientAuthorityTime = 0.15f;
    public static readonly float ShockDuration = 0.5f;
    public static readonly float ShockDamageCooldown = ShockDuration / 3f;

    readonly Collider[] _hitBuffer = new Collider[32];
    readonly HashSet<Player> _distinctHitPlayers = [];
    public readonly Bounds Bounds = new(tesla.transform.position + Vector3.up * (tesla.sizeOfKiller.y / 2f), tesla.transform.rotation * new Vector3(4f, 0.7f, 5f));

    public void Burst() => Timing.RunCoroutine(BurstCoroutine());

    IEnumerator<float> BurstCoroutine()
    {
        Debug.DrawCube(Bounds.center, Bounds.size, Colors.Yellow * 30, ShockDuration);

        yield return Timing.WaitForSeconds(ClientAuthorityTime);

        float shockTime = ShockDuration - ClientAuthorityTime * 2;

        while (shockTime > 0)
        {
            UpdateBurst();
            yield return Timing.WaitForOneFrame;
            shockTime -= Time.deltaTime;
        }
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
            ProcessHit(player);
        }
    }

    void ProcessHit(Player player)
    {
        if (ServersideTeslaDamageModule.ShockedPlayers.TryGetValue(player, out float shockTime) && Time.time - shockTime < ShockDamageCooldown)
            return;
        
        TeslaGateController.ServerReceiveMessage(player.Connection, new(tesla));
    }
}