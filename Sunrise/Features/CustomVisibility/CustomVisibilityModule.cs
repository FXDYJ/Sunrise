using System;
using MapGeneration;
using Sunrise.Utility;

namespace Sunrise.Features.CustomVisibility;

/// <summary>
///     Wallhack nerf. Works by only limiting player visibility to only places that they can see. Reduces wallhack effective distance to 12m from 36m
/// </summary>
public class CustomVisibilityModule : PluginModule
{
    protected override void OnEnabled()
    {
        SeedSynchronizer.OnGenerationStage += OnMapGenerationStage;
    }

    void OnMapGenerationStage(MapGenerationPhase mapGenerationStage)
    {
        if (mapGenerationStage == MapGenerationPhase.RelativePositioningWaypoints)
        {
            foreach (Room room in Room.List)
            {
                try
                {
                    RoomVisibilityData.Get(RoomIdUtils.PositionToCoords(room.Position));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to Get visibility data for room {room.Type} during map generation: {e}");
                }
            }
        }
    }
}