using System.Collections.Generic;
using System.IO;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using Path = osu.Framework.Graphics.Lines.Path;

namespace SlippyMap.Game
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

        private Path createPoly(Polygon poly)
        {
            var path = new Path
            {
                PathRadius = 1,
                Colour = Colour4.Red,
                AutoSizeAxes = Axes.None,
                RelativeSizeAxes = Axes.Both
            };

            List<Vector2> vertices = new List<Vector2>();

            foreach (var lineString in poly.Coordinates)
            {
                foreach (var loc in lineString.Coordinates)
                {
                    var position = getPosition(loc);

                    vertices.Add(position);
                }
            }

            path.Vertices = vertices;

            return path;
        }

        protected override void Update()
        {
            base.Update();

            if (!polyLayout.IsValid)
                draw();
        }

        private void draw()
        {
            foreach (var feature in features.Features)
            {
                switch (feature.Geometry)
                {
                    case MultiPolygon mp:
                        foreach (var polygon in mp.Coordinates)
                            // AddInternal(createPoly(polygon));
                            AddInternal(new DelayedLoadUnloadWrapper(() => createPoly(polygon), 100));

                        break;

                    case Polygon p:
                        // AddInternal(createPoly(p));
                        AddInternal(new DelayedLoadUnloadWrapper(() => createPoly(p), 100));

                        break;
                }
            }

            polyLayout.Validate();
        }
    }
}
