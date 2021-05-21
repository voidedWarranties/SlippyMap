using osu.Framework;
using osu.Framework.Platform;

namespace SlippyMap.Tests
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableHost("visual-tests"))
            using (var game = new SlippyMapTestBrowser())
                host.Run(game);
        }
    }
}
