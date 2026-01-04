using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Rendering
{
    public partial class RenderManager : IRenderManager, IDisposable
    {
        #region grid , scale bar , axis draw
        #region Grid Path Generation
        private Path2D BuildGridPath(IViewSettings currentViewSettings, float GridSpacingDistanceReal)
        {
            Path2D gridPath = new Path2D(500);

            float gridSpacingDistancePixel = currentViewSettings.DIST_Real_to_Pict(GridSpacingDistanceReal);

            if (gridSpacingDistancePixel < 0.1f)
                return gridPath;

            int gridExtent = 20;
            float maxGridDistance = gridExtent * GridSpacingDistanceReal;

            // XY Plane (Z = 0)
            for (int i = 0; i <= gridExtent; i++)
            {
                float coord = i * GridSpacingDistanceReal;

                // Lines parallel to X-axis
                Vector2D p1 = currentViewSettings.RealToPict(
                      new Vector3D(0, coord, 0f), out _);
                Vector2D p2 = currentViewSettings.RealToPict(
                      new Vector3D(maxGridDistance, coord, 0f), out _);

                if (!float.IsNaN(p1.X) && !float.IsNaN(p1.Y) && !float.IsNaN(p2.X) && !float.IsNaN(p2.Y))
                {
                    p1 = currentViewSettings.ClampToGdiRangePoint(p1);
                    p2 = currentViewSettings.ClampToGdiRangePoint(p2);
                    gridPath.MoveTo(p1);
                    gridPath.LineTo(p2);
                }

                // Lines parallel to Y-axis
                Vector2D p3 = currentViewSettings.RealToPict(
                      new Vector3D(coord, 0, 0f), out _);
                Vector2D p4 = currentViewSettings.RealToPict(
                      new Vector3D(coord, maxGridDistance, 0f), out _);

                if (!float.IsNaN(p3.X) && !float.IsNaN(p3.Y) && !float.IsNaN(p4.X) && !float.IsNaN(p4.Y))
                {
                    p3 = currentViewSettings.ClampToGdiRangePoint(p3);
                    p4 = currentViewSettings.ClampToGdiRangePoint(p4);
                    gridPath.MoveTo(p3);
                    gridPath.LineTo(p4);
                }
            }

            // XZ Plane (Y = 0)
            for (int i = 0; i <= gridExtent; i++)
            {
                float coord = i * GridSpacingDistanceReal;

                Vector2D p1 = currentViewSettings.RealToPict(
                      new Vector3D(0, 0f, coord), out _);
                Vector2D p2 = currentViewSettings.RealToPict(
                      new Vector3D(maxGridDistance, 0f, coord), out _);

                if (!float.IsNaN(p1.X) && !float.IsNaN(p1.Y) && !float.IsNaN(p2.X) && !float.IsNaN(p2.Y))
                {
                    p1 = currentViewSettings.ClampToGdiRangePoint(p1);
                    p2 = currentViewSettings.ClampToGdiRangePoint(p2);
                    gridPath.MoveTo(p1);
                    gridPath.LineTo(p2);
                }

                Vector2D p3 = currentViewSettings.RealToPict(
                      new Vector3D(coord, 0f, 0), out _);
                Vector2D p4 = currentViewSettings.RealToPict(
                      new Vector3D(coord, 0f, maxGridDistance), out _);

                if (!float.IsNaN(p3.X) && !float.IsNaN(p3.Y) && !float.IsNaN(p4.X) && !float.IsNaN(p4.Y))
                {
                    p3 = currentViewSettings.ClampToGdiRangePoint(p3);
                    p4 = currentViewSettings.ClampToGdiRangePoint(p4);
                    gridPath.MoveTo(p3);
                    gridPath.LineTo(p4);
                }
            }

            // YZ Plane (X = 0)
            for (int i = 0; i <= gridExtent; i++)
            {
                float coord = i * GridSpacingDistanceReal;

                Vector2D p1 = currentViewSettings.RealToPict(
                      new Vector3D(0f, 0, coord), out _);
                Vector2D p2 = currentViewSettings.RealToPict(
                      new Vector3D(0f, maxGridDistance, coord), out _);

                if (!float.IsNaN(p1.X) && !float.IsNaN(p1.Y) && !float.IsNaN(p2.X) && !float.IsNaN(p2.Y))
                {
                    p1 = currentViewSettings.ClampToGdiRangePoint(p1);
                    p2 = currentViewSettings.ClampToGdiRangePoint(p2);
                    gridPath.MoveTo(p1);
                    gridPath.LineTo(p2);
                }

                Vector2D p3 = currentViewSettings.RealToPict(
                      new Vector3D(0f, coord, 0), out _);
                Vector2D p4 = currentViewSettings.RealToPict(
                      new Vector3D(0f, coord, maxGridDistance), out _);

                if (!float.IsNaN(p3.X) && !float.IsNaN(p3.Y) && !float.IsNaN(p4.X) && !float.IsNaN(p4.Y))
                {
                    p3 = currentViewSettings.ClampToGdiRangePoint(p3);
                    p4 = currentViewSettings.ClampToGdiRangePoint(p4);
                    gridPath.MoveTo(p3);
                    gridPath.LineTo(p4);
                }
            }

            return gridPath;
        }
        #endregion

        #region Axes Path Generation
        public Path2D BuildAxesPath(IViewSettings currentViewSettings)
        {
            Path2D axesPath = new Path2D(20);

            Vector2D origin = currentViewSettings.RealToPict(
                  new Vector3D(0F, 0F, 0F), out _);
            origin = currentViewSettings.ClampToGdiRangePoint(origin);

            float axisLength = 80.0F / currentViewSettings.ZoomFactorAverage;
            float arrowLength = 20.0F / currentViewSettings.ZoomFactorAverage;
            float arrowWidth = 10.0F / currentViewSettings.ZoomFactorAverage;

            // X-axis
            Vector2D xEnd = currentViewSettings.RealToPict(
                  new Vector3D(axisLength, 0F, 0F), out _);
            xEnd = currentViewSettings.ClampToGdiRangePoint(xEnd);
            axesPath.MoveTo(origin);
            axesPath.LineTo(xEnd);

            Vector2D xArrowTip1 = currentViewSettings.RealToPict(
                  new Vector3D(axisLength - arrowLength, arrowWidth, 0F), out _);
            Vector2D xArrowTip2 = currentViewSettings.RealToPict(
                  new Vector3D(axisLength - arrowLength, -arrowWidth, 0F), out _);
            axesPath.MoveTo(xEnd);
            axesPath.LineTo(xArrowTip1);
            axesPath.MoveTo(xEnd);
            axesPath.LineTo(xArrowTip2);

            // Y-axis
            Vector2D yEnd = currentViewSettings.RealToPict(
                  new Vector3D(0F, axisLength, 0F), out _);
            yEnd = currentViewSettings.ClampToGdiRangePoint(yEnd);
            axesPath.MoveTo(origin);
            axesPath.LineTo(yEnd);

            Vector2D yArrowTip1 = currentViewSettings.RealToPict(
                  new Vector3D(arrowWidth, axisLength - arrowLength, 0F), out _);
            Vector2D yArrowTip2 = currentViewSettings.RealToPict(
                  new Vector3D(-arrowWidth, axisLength - arrowLength, 0F), out _);
            axesPath.MoveTo(yEnd);
            axesPath.LineTo(yArrowTip1);
            axesPath.MoveTo(yEnd);
            axesPath.LineTo(yArrowTip2);

            // Z-axis
            Vector2D zEnd = currentViewSettings.RealToPict(
                  new Vector3D(0F, 0F, axisLength), out _);
            zEnd = currentViewSettings.ClampToGdiRangePoint(zEnd);
            axesPath.MoveTo(origin);
            axesPath.LineTo(zEnd);

            Vector2D zArrowTip1 = currentViewSettings.RealToPict(
                  new Vector3D(arrowWidth, 0F, axisLength - arrowLength), out _);
            Vector2D zArrowTip2 = currentViewSettings.RealToPict(
                  new Vector3D(-arrowWidth, 0F, axisLength - arrowLength), out _);
            axesPath.MoveTo(zEnd);
            axesPath.LineTo(zArrowTip1);
            axesPath.MoveTo(zEnd);
            axesPath.LineTo(zArrowTip2);

            return axesPath;
        }
        #endregion
        #region Scale Bar Draw 

        /// <summary>
        /// Draws a scale bar in the bottom-left corner of the control.
        /// </summary>
        /// <param name="g">The Graphics object to draw on.</param>
        public void DrawScaleBar(IViewSettings CurrentViewSettings, IRenderTarget renderTarget, int _scaleBarLengthPixels)
        {
            const float margin = 10;
            const float barHeight = 5F;
            const float fontSize = 10F;

            int targetPixelLength = _scaleBarLengthPixels;
            float realDistance = targetPixelLength / CurrentViewSettings.ZoomFactorAverage;

            float niceDistance = GetProperDistance(realDistance);
            float pixelLength = CurrentViewSettings.DIST_Real_to_Pict(niceDistance);

            float xStart = margin;
            float xEnd = xStart + pixelLength;
            float yPos = CurrentViewSettings.UsableViewport.Height - margin - barHeight;

            renderTarget.DrawLine(new Vector2D(xStart, yPos), new Vector2D(xEnd, yPos), ArgbColor.Black, 1, false);
            renderTarget.DrawLine(new Vector2D(xStart, yPos - barHeight), new Vector2D(xStart, yPos + barHeight), ArgbColor.Black, 1, false);
            renderTarget.DrawLine(new Vector2D(xEnd, yPos - barHeight), new Vector2D(xEnd, yPos + barHeight), ArgbColor.Black, 1, false);
            //    
            string label = $"{niceDistance:F0} Units";
            renderTarget.DrawString(label, new Vector2D(xStart + margin, yPos - fontSize - 4), ArgbColor.Black, "Arial", fontSize);
        }

        /// <summary>
        /// Adjusts a real-world distance to a "Proper" round number (e.g., 1, 5, 10, 50).
        /// </summary>
        /// <param name="distance">The raw real-world distance.</param>
        /// <returns>A rounded, user-friendly distance.</returns>
        private float GetProperDistance(float distance)
        {
            int magnitude = Convert.ToInt32(Math.Floor(Math.Log10(distance)));
            float normalized = distance / Convert.ToSingle(Math.Pow(10, magnitude));
            float niceValue = 0F;

            if (normalized >= 5F)
            {
                niceValue = 10F;
            }
            else if (normalized >= 2F)
            {
                niceValue = 5F;
            }
            else
            {
                niceValue = 1F;
            }

            return niceValue * Convert.ToSingle(Math.Pow(10, magnitude));
        }
        #endregion
        #region Drawing Helpers
        void DrawGrid(IViewSettings currentViewSettings, IRenderTarget renderTarget, float GridSpacingDistanceReal)
        {
            Path2D gridPath = BuildGridPath(currentViewSettings, GridSpacingDistanceReal);
            renderTarget.DrawPath(gridPath, ArgbColor.LightGray, 1F);
        }

        void DrawAxes(IViewSettings currentViewSettings, IRenderTarget renderTarget)
        {
            Path2D axesPath = BuildAxesPath(currentViewSettings);
            renderTarget.DrawPath(axesPath, ArgbColor.Black, 2F);
            //
            IReadOnlyList<PathSegment> segments = axesPath.GetSegments();
            renderTarget.DrawString("X", segments[0].Point, ArgbColor.Blue, "Arial", 12);
            renderTarget.DrawString("Y", segments[4].Point, ArgbColor.Blue, "Arial", 12);
            renderTarget.DrawString("Z", segments[8].Point, ArgbColor.Blue, "Arial", 12);
        }
        #endregion
        #endregion

    }
}
