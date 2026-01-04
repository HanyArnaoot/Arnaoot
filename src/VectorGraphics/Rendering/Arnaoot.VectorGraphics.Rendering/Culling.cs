using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arnaoot.VectorGraphics.Rendering
{
    public partial class RenderManager : IRenderManager, IDisposable
    {
        public bool IsBoundsVisible(BoundingBox3D worldBounds, IViewSettings view)
        {
            return IsVisible(worldBounds, view);
        }
        private static bool IsVisible(BoundingBox3D worldBounds, IViewSettings view)
        {
            if (worldBounds.IsEmpty())
                return false;

            var min = view.RealToPict(worldBounds.Min, out _);
            var max = view.RealToPict(worldBounds.Max, out _);

            if (!min.IsValid || !max.IsValid)
                return false;

            float left = Math.Min(min.X, max.X);
            float top = Math.Min(min.Y, max.Y);
            float right = Math.Max(min.X, max.X);
            float bottom = Math.Max(min.Y, max.Y);

            var screenAABB = new Rect2(left, top, right - left, bottom - top);
            return screenAABB.IntersectsWith(view.UsableViewport);
        }

    }
}
