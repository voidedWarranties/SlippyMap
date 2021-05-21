using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;

namespace SlippyMap.Game
{
    public class Map : CompositeDrawable
    {
        private readonly IResourceStore<byte[]> store;

        private readonly string tilePath;

        public readonly BindableInt ZoomLevel = new BindableInt();

        public TileMetadata Metadata;

        private Container layers;

        private Vector2 topLeftT;
        private int tileWidth;

        public Map(IResourceStore<byte[]> store, string tilePath = "stamen-watercolor")
        {
            this.store = store;

            this.tilePath = tilePath;

            var metadataPath = $"Maps/{tilePath}/metadata.json";

            using (var sr = new StreamReader(store.GetStream(metadataPath)))
            {
                var metadata = JsonConvert.DeserializeObject<TileMetadata>(sr.ReadToEnd());

                if (metadata == null) throw new Exception("Metadata could not be parsed");
                if (metadata.Bounds.Count != 4) throw new Exception("Bounds array should be 4 elements long");

                Metadata = metadata;
            }
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var textures = new TextureStore(host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(store, "Maps")));

            var topLeft = new Vector2((float)Metadata.Bounds[0], (float)Metadata.Bounds[3]);
            var bottomRight = new Vector2((float)Metadata.Bounds[2], (float)Metadata.Bounds[1]);

            topLeftT = OsmMath.LatToTile(topLeft, Metadata.MinZoom);
            var bottomRightT = OsmMath.LatToTile(bottomRight, Metadata.MinZoom);
            tileWidth = 256 / (1 << (Metadata.MinZoom - 5));
            var size = (bottomRightT.Floor() - topLeftT.Floor() + new Vector2(1)) * tileWidth;

            GridContainer gridContainer;
            InternalChildren = new Drawable[]
            {
                gridContainer = new GridContainer
                {
                    Size = size
                },
                layers = new Container
                {
                    Size = size
                }
            };

            List<Drawable> row = new List<Drawable>();
            List<Drawable[]> grid = new List<Drawable[]>();

            for (var y = (int)topLeftT.Y; y <= bottomRightT.Y; y++)
            {
                for (var x = (int)topLeftT.X; x <= bottomRightT.X; x++)
                {
                    row.Add(new ZoomableTile(textures, new Vector2(x, y), Metadata.MinZoom, tilePath, tileWidth)
                    {
                        ZoomLevel = { BindTarget = ZoomLevel }
                    });
                }

                grid.Add(row.ToArray());
                row.Clear();
            }

            gridContainer.Content = grid.ToArray();
        }

        public void AddLayer<T>(T layer)
            where T : Drawable, ILayer
        {
            Schedule(() =>
            {
                layers.Add(layer);
            });
        }

        protected override void Update()
        {
            base.Update();

            foreach (var child in layers.Children)
            {
                var l = child as ILayer;

                Trace.Assert(l != null);

                l.TileSize = tileWidth;
                l.Offset = topLeftT;
                l.MinZoom = Metadata.MinZoom;
            }
        }
    }
}
