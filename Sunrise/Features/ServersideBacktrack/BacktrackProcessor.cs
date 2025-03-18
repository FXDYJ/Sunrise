using System;

namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     A disposable struct that temporarily resets player's position to a backtracked one.
/// </summary>
public readonly struct BacktrackProcessor : IDisposable
{
    readonly BacktrackEntry _previous;
    readonly Player _player;

    public BacktrackProcessor(Player player, BacktrackEntry claimed, bool forecast)
    {
        _player = player;
        _previous = new(_player);

        if (ServersideBacktrackModule.BacktrackHistories.TryGetValue(player, out BacktrackHistory history))
            history.RestoreClosestEntry(claimed, forecast);
    }

    public void Dispose()
    {
        _previous.Restore(_player);
    }
}