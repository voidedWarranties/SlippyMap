using osu.Framework.Testing;

namespace SlippyMap.Game.Tests.Visual
{
    public class SlippyMapTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new SlippyMapTestSceneTestRunner();

        private class SlippyMapTestSceneTestRunner : SlippyMapGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}