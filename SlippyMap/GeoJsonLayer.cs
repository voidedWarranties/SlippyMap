using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using Path = osu.Framework.Graphics.Lines.Path;

namespace SlippyMap
{
    public class GeoJsonLayer : CompositeDrawable, ILayer
    {
        private float tileSize;

        public float TileSize
        {
            get => tileSize;
            set
            {
                if (value == tileSize) return;

                tileSize = value;

                polyLayout.Invalidate();
            }
        }

        private Vector2 offset;

        public Vector2 Offset
        {
            get => offset;
            set
            {
                if (value == offset) return;

                offset = value;

                polyLayout.Invalidate();
            }
        }

        private int minZoom;

        public int MinZoom
        {
            get => minZoom;
            set
            {
                if (value == minZoom) return;

                minZoom = value;

                polyLayout.Invalidate();
            }
        }

        private readonly Cached polyLayout = new Cached();

        private readonly FeatureCollection features;

        public GeoJsonLayer(Stream file)
        {
            RelativeSizeAxes = Axes.Both;

            Anchor = Anchor.TopLeft;

            using (StreamReader sr = new StreamReader(file))
            {
                features = JsonConvert.DeserializeObject<FeatureCollection>(sr.ReadToEnd());
            }
        }

        private Vector2 getPosition(IPosition pos)
        {
            var posVec = new Vector2((float)pos.Longitude, (float)pos.Latitude);
            var tilePos = OsmMath.LatToTile(posVec, MinZoom);

            return (tilePos - Offset.Floor()) * TileSize;
        }

        private void createPoly(Polygon poly)
        {
            var path = new Path
            {
                PathRadius = 1,
                Colour = Colour4.Red
            };

            List<IPosition> positions = new List<IPosition>();

            double minLong = double.MaxValue;
            double maxLat = double.MinValue;

            foreach (var lineString in poly.Coordinates)
            {
                foreach (var loc in lineString.Coordinates)
                {
                    if (loc.Longitude < minLong) minLong = loc.Longitude;
                    if (loc.Latitude > maxLat) maxLat = loc.Latitude;

                    positions.Add(loc);
                }
            }

            var corner = new Position(maxLat, minLong);

            var targetPos = getPosition(corner);
            var vertices = positions.Select(p => getPosition(p) - getPosition(corner)).ToList();

            path.Position = targetPos - Vector2.One * path.PathRadius; // Compensate for the thickness of the line (perhaps inaccurately)
            path.Vertices = vertices;

            AddInternal(new DrawablePolygon
            {
                Position = targetPos,
                Vertices = vertices
            });

            AddInternal(path);
        }

        protected override void Update()
        {
            base.Update();

            if (!polyLayout.IsValid)
                draw();
        }

        // TODO: Use DLUW?
        private void draw()
        {
            foreach (var feature in features.Features)
            {
                switch (feature.Geometry)
                {
                    case MultiPolygon mp:
                        foreach (var polygon in mp.Coordinates)
                            createPoly(polygon);

                        break;

                    case Polygon p:
                        createPoly(p);

                        break;
                }
            }

            polyLayout.Validate();
        }
    }
}
