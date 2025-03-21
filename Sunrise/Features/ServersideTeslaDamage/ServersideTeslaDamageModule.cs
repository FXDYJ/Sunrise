using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using Sunrise.EntryPoint;
using Sunrise.Utility;

namespace Sunrise.Features.ServersideTeslaDamage;

public class ServersideTeslaDamageModule : PluginModule
{
    public static readonly Dictionary<TeslaGate, ServersideTeslaHitreg> TeslaHitreg = new();
    public static readonly Dictionary<Player, float> ShockedPlayers = new();

    protected override void OnEnabled()
    {
        if (!SunrisePlugin.Instance.Config.ServersideTeslaDamage)
            return;

        TeslaGate.OnBursted += OnTeslaGateBursted;
        Handlers.Player.Hurt += OnPlayerHurt;
    }

    static void OnTeslaGateBursted(TeslaGate tesla)
    {
        ServersideTeslaHitreg serversideTeslaHitreg = TeslaHitreg.GetOrAdd(tesla, () => new(tesla));
        serversideTeslaHitreg.Burst();
    }

    static void OnPlayerHurt(HurtEventArgs ev)
    {
        if (ev.DamageHandler.Type == DamageType.Tesla)
            ShockedPlayers[ev.Player] = Time.time;
    }
}