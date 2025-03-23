using Exiled.API.Enums;

namespace Sunrise.Utility;

public static class PlayerExtensions
{
    public static float SqrRenderDistance(this Player player) => player.Position.y > 800 ? (float)5041 : 1369; // 36+1, 70+1

}