using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osuTK;
using SlippyMap.Resources;

namespace SlippyMap.Game.Tests.Visual
{
    public class TestSceneZoomableTile : TestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var dllStore = new DllResourceStore(typeof(SlippyMapResources).Assembly);
            var textures = new TextureStore(host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(dllStore, "Maps")));

            ZoomableTile tile;
            Add(tile = new ZoomableTile(textures, new Vector2(56, 25), 6));

            AddSliderStep("zoom", 6, 8, 6, val =>
            {
                tile.ZoomLevel.Value = val;
            });
        }
    }
}
