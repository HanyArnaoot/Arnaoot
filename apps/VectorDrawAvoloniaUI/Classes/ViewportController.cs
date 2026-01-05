using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Arnaoot.VectorGraphics.Core;
using Arnaoot.VectorGraphics.View;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector3D = Arnaoot.Core.Vector3D;

namespace VectorDrawAvoloniaUI.Classes
{
    /// <summary>
    /// Handles viewport transformations (pan, zoom, rotation)
    /// </summary>
    public class ViewportController
    {
        private readonly DrawingController _drawingController;
        private readonly Zooming _zooming;

        public ViewportController(DrawingController drawingController)
        {
            _drawingController = drawingController ?? throw new ArgumentNullException(nameof(drawingController));
            _zooming = new Zooming();
        }

        public void Pan(PanDirection direction)
        {
            var viewSettings = _drawingController.ViewSettings;
            var shift = viewSettings.ShiftWorld;
            int shiftSize = (int)(5 / viewSettings.ZoomFactorAverage);

            shift = direction switch
            {
                PanDirection.XPlus => new Vector3D(shift.X + shiftSize, shift.Y, shift.Z),
                PanDirection.XMinus => new Vector3D(shift.X - shiftSize, shift.Y, shift.Z),
                PanDirection.YPlus => new Vector3D(shift.X, shift.Y + shiftSize, shift.Z),
                PanDirection.YMinus => new Vector3D(shift.X, shift.Y - shiftSize, shift.Z),
                _ => shift
            };

            var newSettings = new ViewSettings(
                viewSettings.UsableViewport ,
                viewSettings.ZoomFactor,
                shift,
                viewSettings.RotationAngle,
                viewSettings.RotateAroundPoint
            );

            _drawingController.UpdateViewSettings(newSettings);
        }

        public void Zoom(ZoomAction action, Rect canvasBounds)
        {
            var viewSettings = _drawingController.ViewSettings;
            double centerX = canvasBounds.Width / 2;
            double centerY = canvasBounds.Height / 2;

            IViewSettings newSettings = action switch
            {
                ZoomAction.In => _zooming.ZoomIn(viewSettings, (float)centerX, (float)centerY),
                ZoomAction.Out => _zooming.ZoomOut(viewSettings, (float)centerX, (float)centerY),
                ZoomAction.Fit => _zooming.ZoomExtents(viewSettings, _drawingController.LayerManager, 5f),
                _ => viewSettings
            };

            _drawingController.UpdateViewSettings(newSettings);
        }

        public void UpdateRotation(RotationAngles angles, Rect canvasBounds)
        {
            var viewSettings = _drawingController.ViewSettings;

            var viewport = new Rect2(0, 0, (int)canvasBounds.Width, (int)canvasBounds.Height);
            var centerPixel = new Vector2D((float)(viewport.Width / 2.0), (float)(viewport.Height / 2.0));
            var rotatePoint = viewSettings.PictToViewPlane(centerPixel, 0.0f);

            var rotationAngle = new Vector3D(
                angles.X * 3.14f / 180f,
                angles.Y * 3.14f / 180f,
                angles.Z * 3.14f / 180f
            );

            var newSettings = new ViewSettings(
                viewport,
                viewSettings.ZoomFactor,
                viewSettings.ShiftWorld,
                rotationAngle,
                rotatePoint
            );

            _drawingController.UpdateViewSettings(newSettings);
        }
    }

    public enum PanDirection
    {
        XPlus,
        XMinus,
        YPlus,
        YMinus
    }

    public enum ZoomAction
    {
        In,
        Out,
        Fit
    }

    public record RotationAngles(float X, float Y, float Z);
}
