using System;
using System.Collections.Generic;
using System.Linq;
using EarcutNet;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace SlippyMap
{
    public class DrawablePolygon : Drawable
    {
        private readonly List<Vector2> vertices = new List<Vector2>();
        private readonly List<Triangle> triangles = new List<Triangle>();

        public IReadOnlyList<Vector2> Vertices
        {
            set
            {
                vertices.Clear();
                vertices.AddRange(value);

                vertexBoundsCache.Invalidate();
                Invalidate(Invalidation.DrawSize);

                tessellate();
            }
        }

        public override Axes RelativeSizeAxes
        {
            get => base.RelativeSizeAxes;
            set
            {
                if ((AutoSizeAxes & value) != 0)
                    throw new InvalidOperationException("No axis can be relatively sized and automatically sized at the same time.");

                base.RelativeSizeAxes = value;
            }
        }

        private Axes autoSizeAxes;

        public virtual Axes AutoSizeAxes
        {
            get => autoSizeAxes;
            set
            {
                if (value == autoSizeAxes)
                    return;

                if ((RelativeSizeAxes & value) != 0)
                    throw new InvalidOperationException("No axis can be relatively sized and automatically sized at the same time.");

                autoSizeAxes = value;
                OnSizingChanged();
            }
        }

        public override Vector2 Size
        {
            get
            {
                if (AutoSizeAxes != Axes.None)
                    return base.Size = vertexBounds.Size;

                return base.Size;
            }
            set
            {
                if ((AutoSizeAxes & Axes.Both) != 0)
                    throw new InvalidOperationException($"The Size of a {nameof(DrawablePolygon)} with {nameof(AutoSizeAxes)} cannot be set manually.");

                base.Size = value;
            }
        }

        private readonly Cached<RectangleF> vertexBoundsCache = new Cached<RectangleF>();

        private RectangleF vertexBounds
        {
            get
            {
                if (vertexBoundsCache.IsValid)
                    return vertexBoundsCache.Value;

                if (vertices.Count > 0)
                {
                    float minX = 0, minY = 0, maxX = 0, maxY = 0;

                    foreach (var v in vertices)
                    {
                        minX = Math.Min(minX, v.X);
                        minY = Math.Min(minY, v.Y);
                        maxX = Math.Max(maxX, v.X);
                        maxY = Math.Max(maxY, v.Y);
                    }

                    return vertexBoundsCache.Value = new RectangleF(minX, minY, maxX - minX, maxY - minY);
                }

                return vertexBoundsCache.Value = new RectangleF(0, 0, 0, 0);
            }
        }

        public DrawablePolygon()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override DrawNode CreateDrawNode() => new MultiPolygonDrawNode(this);

        private void tessellate()
        {
            triangles.Clear();

            List<double> coords = new List<double>();

            foreach (var coord in vertices)
            {
                coords.Add(coord.X);
                coords.Add(coord.Y);
            }

            var tessellation = Earcut.Tessellate(coords.ToArray(), Array.Empty<int>());

            for (int i = 0; i < tessellation.Count; i += 3)
            {
                var triangle = new Triangle(vertices[tessellation[i]], vertices[tessellation[i + 1]], vertices[tessellation[i + 2]]);

                triangles.Add(triangle);
            }
        }

        private class MultiPolygonDrawNode : DrawNode
        {
            protected new DrawablePolygon Source => (DrawablePolygon)base.Source;

            private List<Triangle> triangles;

            public MultiPolygonDrawNode(IDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                triangles = Source.triangles;
            }

            private Triangle toScreenSpace(Triangle t)
            {
                var p0 = osu.Framework.Graphics.Vector2Extensions.Transform(t.P0, DrawInfo.Matrix);
                var p1 = osu.Framework.Graphics.Vector2Extensions.Transform(t.P1, DrawInfo.Matrix);
                var p2 = osu.Framework.Graphics.Vector2Extensions.Transform(t.P2, DrawInfo.Matrix);

                return new Triangle(p0, p1, p2);
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                var screenTriangles = triangles.Select(toScreenSpace);

                foreach (var triangle in screenTriangles)
                {
                    DrawTriangle(Texture.WhitePixel, triangle, Colour4.Blue.Opacity(0.5f));
                }
            }
        }
    }
}
