using System;

namespace Sunrise.API.Backtracking;

[Flags]
internal enum EntryMatchFlags : byte
{
    Position = 1 << 0,
    Rotation = 1 << 1,

    None = 0,
    All = Position | Rotation,
}