using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using SlippyMap.Resources;

namespace SlippyMap.Tests.Visual
{
    public class TestSceneInteractiveMap : TestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var dllStore = new DllResourceStore(typeof(SlippyMapResources).Assembly);

            InteractiveMap map;

            Add(map = new InteractiveMap(dllStore) { RelativeSizeAxes = Axes.Both });

            map.Map.AddLayer(new GeoJsonLayer(dllStore.GetStream("GeoJSON/japan.json")));
        }
    }
}
