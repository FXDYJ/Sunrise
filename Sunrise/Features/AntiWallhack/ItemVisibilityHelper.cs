using Exiled.API.Features.Items;
using InventorySystem.Items.Coin;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.MicroHID.Modules;
using InventorySystem.Items.Usables;
using Consumable = Exiled.API.Features.Items.Consumable;
using Firearm = Exiled.API.Features.Items.Firearm;

namespace Sunrise.Features.AntiWallhack;

internal static class ItemVisibilityHelper
{
    public static float GetVisibilityRange(Item? item)
    {
        switch (item)
        {
            case null:
            {
                return 0;
            }
            case Firearm firearm:
            {
                if (firearm.Base is ParticleDisruptor disruptor)
                    return disruptor.AllowHolster ? 0 : 100;

                if (firearm.FlashlightEnabled)
                    return 100;
                else if (firearm.IsReloading)
                    return 11.5f;
                else
                    return 0;
            }
            case MicroHid microHid:
            {
                return microHid.State switch
                {
                    MicroHidPhase.Standby => 0,
                    MicroHidPhase.WindingUp or MicroHidPhase.WoundUpSustain or MicroHidPhase.WindingDown => 15,
                    MicroHidPhase.Firing => 45,
                    _ => 0,
                };
            }
            case Consumable { IsUsing: true }:
            {
                // medkit 14.5
                // painkillers 14.5
                // cola 14.5, candy 14.5
                // adrenaline 14.5, 
                // steroids - 14.5
                return 14.5f;
            }
            case Radio { IsEnabled: true }:
            {
                // radio - 11 when receiving
                return 11;
            }
            // coin 5
            // 268 - 5
            case Usable:
            {
                return item.Base switch
                {
                    Scp268 { IsUsing: true } or Coin { _lastUseSw.ElapsedMilliseconds: < 600 } => 5,
                    _ => 0,
                };
            }
            case Throwable throwable:
            {
                return throwable.Base.AllowHolster ? 0 : 5;
            }
            default:
            {
                return 0;
            }
        }
    }
}