using osu.Framework.Platform;
using osu.Framework;
using SlippyMap.Game;

namespace SlippyMap.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableHost(@"SlippyMap"))
            using (osu.Framework.Game game = new SlippyMapGame())
                host.Run(game);
        }
    }
}
