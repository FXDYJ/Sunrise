using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using MapEditorReborn.API.Extensions;
using MapGeneration;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;

namespace Sunrise.Features.CustomVisibility;

// InvisibilityFlags GetActiveFlags(ReferenceHub observer)
/*
.locals init
[0] valuetype PlayerRoles.Visibility.InvisibilityFlags activeFlags,
[1] class PlayerRoles.FirstPersonControl.IFpcRole currentRole1,
[2] class PlayerRoles.FirstPersonControl.IFpcRole currentRole2,
[3] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 position2,
[4] float32 num,
[5] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 V_5
*/
[HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.GetActiveFlags))] [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class VisibilityPatch
{
    private static Dictionary<Vector3Int, HashSet<Vector3Int>> _roomMap = new();
    
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = instructions.ToList();

        newInstructions.InsertRange(newInstructions.Count - 1,
        [
            new(OpCodes.Ldloc_1), // currentRole1 (observer)
            new(OpCodes.Ldloc, 3), // position2 (owner)
            new(OpCodes.Call, Method(typeof(VisibilityPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

    public static bool Enabled = false;
    public static float SmallerRoomGridSize = 9;

    /// <summary>
    ///     This method limits visibility diagonally when players are inside the facility.
    /// </summary>
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole role1, Vector3 position2)
    {
        // players are out of range
        if (!Enabled || flags.HasFlagFast(InvisibilityFlags.OutOfRange))
            return flags;

        Vector3 position1 = role1.FpcModule.Position;
        Vector3Int coords1 = RoomIdUtils.PositionToCoords(position1); // observer
        Vector3Int coords2 = RoomIdUtils.PositionToCoords(position2); // owner

        if (!_roomMap.TryGetValue(coords1, out var roomMap))
            _roomMap[coords1] = roomMap = GetSeeingCoords(Room.Get(position1));
        
        if (!roomMap.Contains(coords2))
            return flags | InvisibilityFlags.OutOfRange;

        return flags;
    }
    
    private static readonly Vector3Int[] Directions =
    [
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left
    ];

    public static HashSet<Vector3Int> GetSeeingCoords(Room startRoom)
    {
        Vector3Int roomCoords = RoomIdUtils.PositionToCoords(startRoom.Position);
        HashSet<Vector3Int> seeing = [roomCoords];

        foreach (Vector3Int direction in Directions)
        {
            Room currentRoom = startRoom;
            Vector3Int currentCoords = roomCoords;
            
            while (true)
            {
                Room nextRoom = null;
                
                foreach (Room room in currentRoom.NearestRooms)
                {
                    Vector3Int coords = RoomIdUtils.PositionToCoords(room.Position);

                    if (coords - currentCoords == direction)
                    {
                        seeing.Add(coords);
                        nextRoom = room;
                        currentCoords = coords;
                        break;
                    }
                }
                
                if (nextRoom == null)
                    break;
                
                currentRoom = nextRoom;
            }
        }
        
        return seeing;
    }
}