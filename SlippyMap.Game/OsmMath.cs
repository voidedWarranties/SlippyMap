using System;
using osuTK;

namespace SlippyMap.Game
{
    /// <summary>
    /// A summary of these calculations can be found on the OpenStreetMap wiki.
    /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
    /// https://wiki.openstreetmap.org/wiki/Zoom_levels
    /// </summary>
    public static class OsmMath
    {
        /// <summary>
        /// Utility function to convert world coordinates (longitude, latitude) into tile coordinates (x, y)
        /// </summary>
        /// <param name="lonlat">Vector with X being longitude and Y being latitude</param>
        /// <param name="zoom">Zoom factor</param>
        /// <returns>Tile coordinates in float form. Must be converted into integers (floor) to be used for rendering.</returns>
        public static Vector2 LatToTile(Vector2 lonlat, int zoom)
        {
            var n = 1 << zoom;

            var tilex = n * (lonlat.X + 180) / 360;

            var latRad = lonlat.Y * Math.PI / 180;
            var tiley = n * (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2;

            return new Vector2(tilex, (float)tiley);
        }

        /// <summary>
        /// Utility function to convert tile coordinates (x, y) into world coordinates (longitude, latitude)
        /// </summary>
        /// <param name="tile">Vector with X being tilex and Y being tiley</param>
        /// <param name="zoom">Zoom factor</param>
        /// <returns>Vector with X being longitude and Y being latitude</returns>
        public static Vector2 TileToLat(Vector2 tile, int zoom)
        {
            var n = 1 << zoom;

            var lon = tile.X / n * 360 - 180;

            var latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * tile.Y / n)));
            var lat = (float)(latRad * 180 / Math.PI);

            return new Vector2(lon, lat);
        }

        /// <summary>
        /// Zoom from one tile coordinate to another
        /// </summary>
        /// <param name="tile">Tile coordinates in current zoom factor</param>
        /// <param name="from">Current zoom factor</param>
        /// <param name="to">Target zoom factor</param>
        /// <returns>Tile coordinates in target zoom factor</returns>
        public static Vector2 Zoom(Vector2 tile, int from, int to)
        {
            var lonlat = TileToLat(tile, from);

            return LatToTile(lonlat, to);
        }
    }
}
