using osuTK;

namespace SlippyMap.Game
{
    public interface ILayer
    {
        float TileSize { get; set; }

        Vector2 Offset { get; set; }

        int MinZoom { get; set; }
    }
}
