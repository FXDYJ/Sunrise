using System;

namespace Sunrise.API.Backtracking;

/// <summary>
///     A disposable struct that temporarily resets player's position to a backtracked one, then restores it back.
/// </summary>
internal readonly struct BacktrackProcessor : IDisposable
{
    readonly BacktrackEntry _previous;
    readonly Player _player;

    internal BacktrackProcessor(Player player, BacktrackEntry claimed, bool forecast)
    {
        _player = player;
        _previous = new(_player);
        
        BacktrackHistory.Get(player)
            .RestoreClosestEntry(claimed, forecast); 
    }

    public void Dispose() // BUG: Prevents teleports inside OnShooting from working if player dies. is it possible by default tho?
    {
        _previous.Restore(_player);
    }
}