using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using SkiaSharp;
using System;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Platform.Skia
{
    public sealed partial class SkiaRenderTarget : IRenderTarget, IDisposable
     {
        #region IRenderTarget Implementation
        public void Clear(ArgbColor color)
        {
            if (_canvas == null)
            {
                throw new InvalidOperationException(
                    "Clear() called before BeginFrame(), or BeginFrame failed. " +
                    "Ensure BeginFrame(width, height) is called and succeeds.");
            }

            _canvas.Clear(new SKColor(color.R, color.G, color.B, color.A));
        }

        public void DrawLine(Vector2D p1, Vector2D p2, ArgbColor color, float width, bool isSelected = false)
        {
            var paint = GetCachedPaint(color, width, true, isSelected);
            _canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y, paint);
        }

        public void DrawEllipse(Vector2D center, float radiusX, float radiusY, float angleRad,
                                ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            if (radiusX < 1 || radiusY < 1)
                return;

            _canvas.Save();
            try
            {
                _canvas.Translate(center.X, center.Y);
                _canvas.RotateRadians(angleRad);

                var rect = SKRect.Create(-radiusX, -radiusY, radiusX * 2, radiusY * 2);

                if (fill.HasValue)
                {
                    var fillPaint = GetCachedPaint(fill.Value, 0, false);
                    _canvas.DrawOval(rect, fillPaint);
                }

                var strokePaint = GetCachedPaint(stroke, strokeWidth, true);
                _canvas.DrawOval(rect, strokePaint);
            }
            finally
            {
                _canvas.Restore();
            }
        }

        public void DrawRectangle(Rect2 rect, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            var skRect = SKRect.Create(rect.X, rect.Y, rect.Width, rect.Height);

            if (fill.HasValue)
            {
                var fillPaint = GetCachedPaint(fill.Value, 0, false);
                _canvas.DrawRect(skRect, fillPaint);
            }

            var strokePaint = GetCachedPaint(stroke, strokeWidth, true);
            _canvas.DrawRect(skRect, strokePaint);
        }

        public void DrawPolygon(ReadOnlySpan<Vector2D> points, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            if (points.Length < 3) return;

            using (var path = new SKPath())
            {
                path.MoveTo(points[0].X, points[0].Y);
                for (int i = 1; i < points.Length; i++)
                {
                    path.LineTo(points[i].X, points[i].Y);
                }
                path.Close();

                if (fill.HasValue)
                {
                    var fillPaint = GetCachedPaint(fill.Value, 0, false);
                    _canvas.DrawPath(path, fillPaint);
                }

                var strokePaint = GetCachedPaint(stroke, strokeWidth, true);
                _canvas.DrawPath(path, strokePaint);
            }
        }

        public void DrawString(string text, Vector2D position, ArgbColor color, string fontFamily = "Arial", float size = 12)
        {
            var typeface = GetCachedTypeface(fontFamily);
            using (var paint = new SKPaint())
            {
                paint.Color = new SKColor(color.R, color.G, color.B, color.A);
                paint.IsAntialias = true;
                paint.TextSize = size;
                paint.Typeface = typeface;

                _canvas.DrawText(text, position.X, position.Y + size, paint); // Adjust Y for baseline
            }
        }

        public void DrawPath(Path2D path, ArgbColor stroke, float strokeWidth, ArgbColor? fill = null)
        {
            if (path == null || path.SegmentCount == 0)
                return;

            var figures = path.GetFigures();
            var segments = path.GetSegments();

            if (figures.Count == 0)
                return;

            var strokePaint = GetCachedPaint(stroke, strokeWidth, true);

            // Render each figure separately
            foreach (var figure in figures)
            {
                using (var skPath = new SKPath())
                {
                    // Start at figure's start point
                    skPath.MoveTo(figure.StartPoint.X, figure.StartPoint.Y);

                    // Add all segments for this figure
                    for (int i = 0; i < figure.SegmentCount; i++)
                    {
                        var segment = segments[figure.SegmentStartIndex + i];
                        switch (segment.Type)
                        {
                            case PathSegmentType.LineTo:
                                skPath.LineTo(segment.Point.X, segment.Point.Y);
                                break;

                            // Future support for curves:
                            // case PathSegmentType.QuadraticBezier:
                            //     skPath.QuadTo(segment.ControlPoint1.X, segment.ControlPoint1.Y, 
                            //                   segment.Point.X, segment.Point.Y);
                            //     break;
                            // case PathSegmentType.CubicBezier:
                            //     skPath.CubicTo(segment.ControlPoint1.X, segment.ControlPoint1.Y,
                            //                    segment.ControlPoint2.X, segment.ControlPoint2.Y,
                            //                    segment.Point.X, segment.Point.Y);
                            //     break;
                            // case PathSegmentType.Arc:
                            //     // Handle arc segments
                            //     break;

                            default:
                                throw new NotSupportedException($"Path segment type {segment.Type} is not yet supported");
                        }
                    }

                    // Close path if needed
                    if (figure.IsClosed)
                    {
                        skPath.Close();
                    }

                    // Fill if closed and fill color provided
                    if (figure.IsClosed && fill.HasValue)
                    {
                        var fillPaint = GetCachedPaint(fill.Value, 0, false);
                        _canvas.DrawPath(skPath, fillPaint);
                    }

                    // Draw outline
                    _canvas.DrawPath(skPath, strokePaint);
                }
            }
        }
        #endregion

    }
}
