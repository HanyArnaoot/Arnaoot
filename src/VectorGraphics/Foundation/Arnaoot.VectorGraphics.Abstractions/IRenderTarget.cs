using Arnaoot.Core;
using Arnaoot.VectorGraphics.Core;
using System;

namespace Arnaoot.VectorGraphics.Abstractions
{
    public partial class Abstractions

    {

        // ========================================
        // IRenderTarget Interface (Cross-Platform)
        // ========================================
        public interface IRenderTarget
        {
            void Clear(ArgbColor color);
            void DrawLine(Vector2D p1, Vector2D p2, ArgbColor color, float width, bool isSelected = false);
            void DrawEllipse(Vector2D center, float radiusX, float radiusY, float angleRad,
                             ArgbColor stroke, float strokeWidth, ArgbColor? fill = null);
            void DrawRectangle(Rect2 rect, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null);
            void DrawPolygon(ReadOnlySpan<Vector2D> points, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null);
            void DrawString(string text, Vector2D position, ArgbColor color, string fontFamily = "Arial", float size = 12);
            void DrawPath(Path2D path, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null);
            void DrawImage(object image, float x, float y, float width, float height);

            void BeginFrame(int width, int height);
            void BeginFrameFromCache();  // NEW: Start with cached scene
            void EndFrame();

            bool TryGetPixelData(out PixelData data);
            void EndScene();  // called *after* scene, before overlays — triggers caching if full render
        }

    }
}
