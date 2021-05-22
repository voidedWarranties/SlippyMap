using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osuTK;

namespace SlippyMap
{
    public class InteractiveMap : CompositeDrawable
    {
        public readonly Map Map;

        public InteractiveMap(IResourceStore<byte[]> store, string tilePath = "stamen-watercolor")
        {
            Masking = true;

            InternalChild = Map = new Map(store, tilePath)
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        // https://stackoverflow.com/a/17738233
        protected override bool OnScroll(ScrollEvent e)
        {
            var mouseRelative = e.MousePosition - Map.Position;

            var lastScale = Map.Scale;
            Map.Scale += e.ScrollDelta.Y * 0.25f * Map.Scale;

            var mouseScaled = mouseRelative * new Vector2(Map.Scale.X / lastScale.X, Map.Scale.Y / lastScale.Y);
            Map.Position += mouseRelative - mouseScaled;

            Map.ZoomLevel.Value = Math.Min(Map.Metadata.MinZoom + (int)Math.Log(Map.Scale.X, 2), Map.Metadata.MaxZoom);

            return base.OnScroll(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            Map.Position += e.Delta;
        }
    }
}
