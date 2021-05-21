using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osuTK;

namespace SlippyMap
{
    /// <summary>
    /// A map tile that will render its own subdivisions (4 for each tile) automatically when zoom level is changed.
    /// The top level tiles should all be at the smallest zoom level in the set.
    /// </summary>
    public class ZoomableTile : CompositeDrawable
    {
        private readonly string tilePath;

        private readonly Vector2 tilePosition;

        private readonly int baseZoom;

        public readonly BindableInt ZoomLevel = new BindableInt();

        private DelayedLoadUnloadWrapper tile;

        private GridContainer zoomedGrid;

        private readonly float size;

        private bool createdGrid;

        private readonly TextureStore textures;

        public ZoomableTile(TextureStore textures, Vector2 tilePosition, int baseZoom, string tilePath = "stamen-watercolor", float size = 256)
        {
            this.textures = textures;
            this.tilePosition = tilePosition;
            this.baseZoom = baseZoom;

            this.tilePath = tilePath;
            this.size = size;

            Size = new Vector2(size);
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            InternalChildren = new Drawable[]
            {
                tile = new DelayedLoadUnloadWrapper(() => new Sprite
                {
                    Size = new Vector2(size),
                    Texture = textures.Get($"{tilePath}/_{baseZoom}/_{(int)tilePosition.X}/{(int)tilePosition.Y}", WrapMode.ClampToEdge, WrapMode.ClampToEdge)
                }, 100),
                zoomedGrid = new GridContainer
                {
                    Size = new Vector2(size)
                }
            };

            ZoomLevel.ValueChanged += e => updateZoom();
            updateZoom();
        }

        private DelayedLoadUnloadWrapper genGridTile(int offsetX, int offsetY)
        {
            var targetZoom = baseZoom + 1;

            return new DelayedLoadUnloadWrapper(() =>
            {
                var pos = OsmMath.Zoom(tilePosition, baseZoom, targetZoom) + new Vector2(offsetX, offsetY);
                var subTile = new ZoomableTile(textures, pos, targetZoom, tilePath, size / 2f)
                {
                    ZoomLevel = { BindTarget = ZoomLevel }
                };

                return subTile;
            }, 100);
        }

        private void updateZoom()
        {
            if (ZoomLevel.Value > baseZoom)
            {
                if (!createdGrid)
                {
                    zoomedGrid.Content = new[]
                    {
                        new Drawable[] { genGridTile(0, 0), genGridTile(1, 0) },
                        new Drawable[] { genGridTile(0, 1), genGridTile(1, 1) }
                    };

                    createdGrid = true;
                }

                tile.FadeOut();
                zoomedGrid.FadeIn();
            }
            else
            {
                zoomedGrid.FadeOut();
                tile.FadeIn();
            }
        }
    }
}
