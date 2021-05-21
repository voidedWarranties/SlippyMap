﻿using System;
using System.Collections.Generic;
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

        public Map(IResourceStore<byte[]> store, string tilePath = "stamen-watercolor")
        {
            this.store = store;

            this.tilePath = tilePath;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var textures = new TextureStore(host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(store, "Maps")));
            var metadataPath = $"Maps/{tilePath}/metadata.json";

            using (var sw = new StreamReader(store.GetStream(metadataPath)))
            {
                var metadata = JsonConvert.DeserializeObject<TileMetadata>(sw.ReadToEnd());

                if (metadata == null) throw new Exception("Metadata could not be parsed");
                if (metadata.Bounds.Count != 4) throw new Exception("Bounds array should be 4 elements long");

                var topLeft = new Vector2((float)metadata.Bounds[0], (float)metadata.Bounds[3]);
                var bottomRight = new Vector2((float)metadata.Bounds[2], (float)metadata.Bounds[1]);

                var topLeftT = OsmMath.LatToTile(topLeft, metadata.MinZoom);
                var bottomRightT = OsmMath.LatToTile(bottomRight, metadata.MinZoom);
                var tileWidth = 256 / (1 << (metadata.MinZoom - 5));

                List<Drawable> row = new List<Drawable>();
                List<Drawable[]> grid = new List<Drawable[]>();

                for (var y = (int)topLeftT.Y; y <= bottomRightT.Y; y++)
                {
                    for (var x = (int)topLeftT.X; x <= bottomRightT.X; x++)
                    {
                        row.Add(new ZoomableTile(textures, new Vector2(x, y), metadata.MinZoom, tilePath, tileWidth)
                        {
                            ZoomLevel = { BindTarget = ZoomLevel }
                        });
                    }

                    grid.Add(row.ToArray());
                    row.Clear();
                }

                InternalChild = new GridContainer
                {
                    Content = grid.ToArray(),
                    Size = (bottomRightT - topLeftT + new Vector2(1)) * tileWidth
                };
            }
        }
    }
}
