using System;
using MapGeneration;
using Sunrise.Utility;

namespace Sunrise.Features.CustomVisibility;

/// <summary>
///     Wallhack nerf.
///     Works by only sending data about players in rooms that can be seen from the room the observer is currently in.
///     Reduces wallhack effective distance to around 12m (from 36m in base game)
/// </summary>
public class CustomVisibilityModule : PluginModule
{
    protected override void OnEnabled()
    {
        SeedSynchronizer.OnGenerationStage += OnMapGenerationStage;
    }

    protected override void OnDisabled()
    {
        SeedSynchronizer.OnGenerationStage -= OnMapGenerationStage;
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