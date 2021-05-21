using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlippyMap
{
    /// <summary>
    /// https://github.com/mapbox/tilejson-spec/tree/master/2.2.0
    /// Not a complete or correct implementation.
    /// </summary>
    public class TileMetadata
    {
        [JsonProperty("minzoom")]
        public int MinZoom { get; set; }

        [JsonProperty("maxzoom")]
        public int MaxZoom { get; set; }

        /// <summary>
        /// Left, bottom, right, top
        /// Required for the purposes of this application.
        /// </summary>
        [JsonProperty("bounds")]
        public List<double> Bounds { get; set; }
    }
}
