using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using SlippyMap.Resources;

namespace SlippyMap.Game.Tests.Visual
{
    public class TestSceneMap : TestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var dllStore = new DllResourceStore(typeof(SlippyMapResources).Assembly);

            Map map;
            Add(map = new Map(dllStore)
            {
                RelativeSizeAxes = Axes.Both
            });

            map.AddLayer(new GeoJsonLayer(dllStore.GetStream(@"GeoJSON/japan.json")));

            AddSliderStep("zoom", 6, 8, 6, val =>
            {
                map.ZoomLevel.Value = val;
            });
        }
    }
}
