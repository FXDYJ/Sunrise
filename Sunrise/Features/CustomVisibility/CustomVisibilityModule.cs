using System;
using MapGeneration;
using Sunrise.Utility;

namespace Sunrise.Features.CustomVisibility;

public class CustomVisibilityModule : PluginModule
{
    protected override void OnEnabled()
    {
        SeedSynchronizer.OnGenerationStage += OnMapGenerationStage;
    }

    void OnMapGenerationStage(MapGenerationPhase mapGenerationStage)
    {
        Log.Warn($"Map generation stage: {mapGenerationStage}");

        if (mapGenerationStage == MapGenerationPhase.RelativePositioningWaypoints)
        {
            Log.Warn($"Generating room visibility data for {Room.List.Count} rooms");

            foreach (Room room in Room.List)
            {
                try
                {
                    Log.Warn($"Generating visibility data for room {room.Type} at {room.Position}");
                    RoomVisibilityData.Get(RoomIdUtils.PositionToCoords(room.Position));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to generate visibility data for room {room.Type}: {e}");
                }
            }
        }
    }
}