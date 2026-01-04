// File: src/VectorGraphics/Foundation/Arnaoot.VectorGraphics.Core/Clipping/LineClipper.cs
namespace Arnaoot.VectorGraphics.Core.Clipping
{
    using Arnaoot.Core;
    using System;

    public interface IScreenSpaceClipper
    {
        /// <summary>
        /// Clips a line in SCREEN/PIXEL coordinates using Liang-Barsky algorithm.
        /// Use this AFTER RealToPict() transformation.
        /// </summary>
        /// <param name="start">Starting point of the line in screen coordinates</param>
        /// <param name="end">Ending point of the line in screen coordinates</param>
        /// <param name="screenBounds">Screen bounds rectangle</param>
        /// <param name="clippedStart">Output: Clipped starting point</param>
        /// <param name="clippedEnd">Output: Clipped ending point</param>
        /// <returns>True if the line is visible after clipping, false otherwise</returns>
        bool ClipLineScreenSpace(
            Vector2D start,
            Vector2D end,
            Rect2 screenBounds,
            out Vector2D clippedStart,
            out Vector2D clippedEnd);
    }
    /// <summary>
    /// Line clipping algorithms (Cohen-Sutherland, Liang-Barsky)
    /// </summary>
    /// using HexagonPathPlan.Core;
    public static class ScreenSpaceClipper
            {
                /// <summary>
                /// Clips a line in SCREEN/PIXEL coordinates using Liang-Barsky.
                /// Use this AFTER RealToPict() transformation.
                /// </summary>
                public static bool ClipLineScreenSpace(
                 Vector2D start,
                  Vector2D end,
                    Rect2 screenBounds,
                    out Vector2D clippedStart,
                    out Vector2D clippedEnd)
                {
                    // Handle NaN early
                    if (float.IsNaN(start.X) || float.IsNaN(start.Y) ||
                        float.IsNaN(end.X) || float.IsNaN(end.Y))
                    {
                        clippedStart = clippedEnd =Vector2D.Empty;
                        return false;
                    }

                    float t0 = 0.0f;
                    float t1 = 1.0f;

                    float dx = end.X - start.X;
                    float dy = end.Y - start.Y;

                    // Liang-Barsky against screen rectangle
                    if (!ClipTest(-dx, start.X - screenBounds.Left, ref t0, ref t1) ||
                        !ClipTest(dx, screenBounds.Right - start.X, ref t0, ref t1) ||
                        !ClipTest(-dy, start.Y - screenBounds.Top, ref t0, ref t1) ||
                        !ClipTest(dy, screenBounds.Bottom - start.Y, ref t0, ref t1))
                    {
                        clippedStart = clippedEnd =Vector2D.Empty;
                        return false;
                    }

                    clippedStart = new Vector2D(start.X + t0 * dx, start.Y + t0 * dy);
                    clippedEnd = new Vector2D(start.X + t1 * dx, start.Y + t1 * dy);

                    return true;
                }

                /// <summary>
                /// Fast visibility test for screen-space line.
                /// </summary>

                private static bool ClipTest(float p, float q, ref float t0, ref float t1)
                {
                    const float EPSILON = 1e-6f;

                    if (Math.Abs(p) < EPSILON)
                        return q >= 0;

                    float r = q / p;

                    if (p < 0)
                    {
                        if (r > t1) return false;
                        if (r > t0) t0 = r;
                    }
                    else
                    {
                        if (r < t0) return false;
                        if (r < t1) t1 = r;
                    }

                    return true;
                }


            }

            //class Clipping
            //{
            //    /// <summary>
            //    /// Implements Liang-Barsky parametric line clipping algorithm for 2D and 3D
            //    /// </summary>
            //    public static class LiangBarskyClipper
            //    {
            //        /// <summary>
            //        /// Clips a line segment against a 2D rectangular boundary.
            //        /// </summary>
            //        /// <param name="start">Start point of the line</param>
            //        /// <param name="end">End point of the line</param>
            //        /// <param name="clipBounds">Clipping rectangle (in same coordinate space)</param>
            //        /// <param name="clippedStart">Clipped start point (output)</param>
            //        /// <param name="clippedEnd">Clipped end point (output)</param>
            //        /// <returns>True if line is visible (at least partially), false if completely clipped</returns>
            //        public static bool ClipLine2D(
            //            Core.Vector3D start,
            //           Vector3D end,
            //           System.Drawing.RectangleF clipBounds,
            //            outVector3D clippedStart,
            //            outVector3D clippedEnd)
            //        {
            //            float t0 = 0.0f;
            //            float t1 = 1.0f;

            //            float dx = end.X - start.X;
            //            float dy = end.Y - start.Y;

            //            // Check all four boundaries using Liang-Barsky algorithm
            //            // For each boundary, calculate parameter t where line intersects

            //            // Left boundary (x = xmin)
            //            if (!ClipTest(-dx, start.X - clipBounds.Left, ref t0, ref t1))
            //            {
            //                clippedStart = clippedEnd = default;
            //                return false;
            //            }

            //            // Right boundary (x = xmax)
            //            if (!ClipTest(dx, clipBounds.Right - start.X, ref t0, ref t1))
            //            {
            //                clippedStart = clippedEnd = default;
            //                return false;
            //            }

            //            // Bottom boundary (y = ymin)
            //            if (!ClipTest(-dy, start.Y - clipBounds.Top, ref t0, ref t1))
            //            {
            //                clippedStart = clippedEnd = default;
            //                return false;
            //            }

            //            // Top boundary (y = ymax)
            //            if (!ClipTest(dy, clipBounds.Bottom - start.Y, ref t0, ref t1))
            //            {
            //                clippedStart = clippedEnd = default;
            //                return false;
            //            }

            //            // Calculate clipped endpoints using parametric form
            //            // P(t) = P0 + t * (P1 - P0)
            //            clippedStart = new Core.Vector3D(
            //                start.X + t0 * dx,
            //                start.Y + t0 * dy,
            //                start.Z + t0 * (end.Z - start.Z) // Interpolate Z
            //            );

            //            clippedEnd = newVector3D(
            //                start.X + t1 * dx,
            //                start.Y + t1 * dy,
            //                start.Z + t1 * (end.Z - start.Z)
            //            );

            //            return true;
            //        }

            //        /// <summary>
            //        /// Clips a line segment against a 3D bounding box.
            //        /// </summary>
            //        public static bool ClipLine3D(
            //           Vector3D start,
            //           Vector3D end,
            //           BoundingBox3D clipBounds,
            //            outVector3D clippedStart,
            //            outVector3D clippedEnd)
            //        {
            //            float t0 = 0.0f;
            //            float t1 = 1.0f;

            //            float dx = end.X - start.X;
            //            float dy = end.Y - start.Y;
            //            float dz = end.Z - start.Z;

            //            // Clip against all 6 planes of the bounding box
            //            if (!ClipTest(-dx, start.X - clipBounds.Min.X, ref t0, ref t1) ||
            //                !ClipTest(dx, clipBounds.Max.X - start.X, ref t0, ref t1) ||
            //                !ClipTest(-dy, start.Y - clipBounds.Min.Y, ref t0, ref t1) ||
            //                !ClipTest(dy, clipBounds.Max.Y - start.Y, ref t0, ref t1) ||
            //                !ClipTest(-dz, start.Z - clipBounds.Min.Z, ref t0, ref t1) ||
            //                !ClipTest(dz, clipBounds.Max.Z - start.Z, ref t0, ref t1))
            //            {
            //                clippedStart = clippedEnd = default;
            //                return false;
            //            }

            //            // Calculate clipped endpoints
            //            clippedStart = newVector3D(
            //                start.X + t0 * dx,
            //                start.Y + t0 * dy,
            //                start.Z + t0 * dz
            //            );

            //            clippedEnd = newVector3D(
            //                start.X + t1 * dx,
            //                start.Y + t1 * dy,
            //                start.Z + t1 * dz
            //            );

            //            return true;
            //        }

            //        /// <summary>
            //        /// Core Liang-Barsky clipping test for a single boundary.
            //        /// </summary>
            //        /// <param name="p">Directional component (can be negative)</param>
            //        /// <param name="q">Distance from boundary</param>
            //        /// <param name="t0">Entry parameter (updated)</param>
            //        /// <param name="t1">Exit parameter (updated)</param>
            //        /// <returns>False if line is completely outside, true otherwise</returns>
            //        private static bool ClipTest(float p, float q, ref float t0, ref float t1)
            //        {
            //            if (Math.Abs(p) < 1e-6f) // Line is parallel to boundary
            //            {
            //                // If q < 0, line is outside boundary
            //                return q >= 0;
            //            }

            //            float r = q / p;

            //            if (p < 0) // Entering the boundary
            //            {
            //                if (r > t1)
            //                    return false; // Line enters after it exits
            //                if (r > t0)
            //                    t0 = r; // Update entry point
            //            }
            //            else // p > 0, exiting the boundary
            //            {
            //                if (r < t0)
            //                    return false; // Line exits before it enters
            //                if (r < t1)
            //                    t1 = r; // Update exit point
            //            }

            //            return true;
            //        }

            //        /// <summary>
            //        /// Quick visibility check without calculating clipped points.
            //        /// Useful for culling before expensive draw operations.
            //        /// </summary>
            //        public static bool IsLineVisible(HexagonPathPlan.Core.Vector3D start,Vector3D end,System.Drawing.RectangleF clipBounds)
            //        {
            //            float t0 = 0.0f;
            //            float t1 = 1.0f;

            //            float dx = end.X - start.X;
            //            float dy = end.Y - start.Y;

            //            return ClipTest(-dx, start.X - clipBounds.Left, ref t0, ref t1) &&
            //                   ClipTest(dx, clipBounds.Right - start.X, ref t0, ref t1) &&
            //                   ClipTest(-dy, start.Y - clipBounds.Top, ref t0, ref t1) &&
            //                   ClipTest(dy, clipBounds.Bottom - start.Y, ref t0, ref t1);
            //        }
            //    }

            //    // Extension to your LineElement class
            //    public static class LineElementExtensions
            //    {
            //        /// <summary>
            //        /// Gets the clipped line coordinates for drawing, or null if completely clipped.
            //        /// This should be called before drawing to avoid rendering off-screen geometry.
            //        /// </summary>
            //        public static (HexagonPathPlan.Core.Vector3D start,Vector3D end)? GetClippedCoordinates(
            //            this LineElement line,
            //           System .Drawing .RectangleF viewportBounds)
            //        {
            //            if (LiangBarskyClipper.ClipLine2D(
            //                line.StartPoint,
            //                line.EndPoint,
            //                viewportBounds,
            //                outVector3D clippedStart,
            //                outVector3D clippedEnd))
            //            {
            //                return (clippedStart, clippedEnd);
            //            }

            //            return null; // Line is completely outside viewport
            //        }
            //    }

            //    // Modified LineElement.Draw method (example integration)
            //    public partial class LineElement
            //    {
            //        /// <summary>
            //        /// Optimized Draw method with Liang-Barsky clipping
            //        /// </summary>
            //        public void DrawWithClipping(System.Drawing.Graphics g, coordinate.ViewSettings CurrentViewSetting, Caching_Helper caching_Helper)
            //        {
            //            // Get viewport bounds in real-world coordinates
            //            System.Drawing.RectangleF viewportBounds = CurrentViewSetting.GetVisibleBounds();

            //            // Clip the line before converting to pixel coordinates
            //            var clipped = this.GetClippedCoordinates(viewportBounds);

            //            if (!clipped.HasValue)
            //                return; // Line is completely outside viewport - skip drawing

            //            // Convert clipped coordinates to pixel space
            //           System.Drawing.HexagonPathPlan .Core.Vector2 startPixel = CurrentViewSetting.RealToPict(clipped.Value.start, out float depth1);
            //            HexagonPathPlan .Core.Vector2 endPixel = CurrentViewSetting.RealToPict(clipped.Value.end, out float depth2);

            //            // Safety checks (should rarely trigger after clipping)
            //            if (float.IsNaN(startPixel.X) || float.IsNaN(startPixel.Y) ||
            //                float.IsNaN(endPixel.X) || float.IsNaN(endPixel.Y))
            //                return;

            //            // Clamp to GDI+ safe range
            //            startPixel.X = coordinate.ClampToGdiRange(startPixel.X);
            //            startPixel.Y = coordinate.ClampToGdiRange(startPixel.Y);
            //            endPixel.X = coordinate.ClampToGdiRange(endPixel.X);
            //            endPixel.Y = coordinate.ClampToGdiRange(endPixel.Y);

            //            // Draw the clipped line
            //            using (System.Drawing.Pen pen = caching_Helper.GetCachedPen(DrawColor, DrawWidth, IsSelected))
            //            {
            //                try
            //                {
            //                    g.DrawLine(pen, startPixel.X, startPixel.Y, endPixel.X, endPixel.Y);
            //                }
            //                catch (ArgumentException)
            //                {
            //                    System.Diagnostics.Debugger.Break();
            //                }
            //            }
            //        }
            //    }
            //}
        }
    
